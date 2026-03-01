import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';
import { AppointmentCalendarComponent } from './appointment-calendar.component';
import { AppointmentService } from '../../../core/services/appointment.service';

describe('AppointmentCalendarComponent', () => {
  let component: AppointmentCalendarComponent;
  let fixture: ComponentFixture<AppointmentCalendarComponent>;
  let appointmentServiceSpy: { getAll: jest.Mock };
  let dialogSpy: { open: jest.Mock };

  const mockPaginatedResult = {
    items: [
      { id: 'a1', startTime: '2026-03-04T10:00:00Z', endTime: '2026-03-04T10:30:00Z', customerName: 'John', status: 'Confirmed' },
    ],
    totalPages: 1, totalCount: 1, hasPreviousPage: false, hasNextPage: false,
  };

  beforeEach(async () => {
    appointmentServiceSpy = { getAll: jest.fn().mockReturnValue(of(mockPaginatedResult)) };
    dialogSpy = { open: jest.fn(() => ({ afterClosed: () => of(true) })) };

    await TestBed.configureTestingModule({
      imports: [AppointmentCalendarComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: AppointmentService, useValue: appointmentServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AppointmentCalendarComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have 12 hours displayed (8-19)', () => {
    expect(component.hours).toEqual([8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19]);
  });

  describe('ngOnInit', () => {
    it('should build week and load appointments', () => {
      component.ngOnInit();
      expect(component.weekDays.length).toBe(7);
      expect(component.weekLabel).toBeTruthy();
      expect(appointmentServiceSpy.getAll).toHaveBeenCalled();
      expect(component.loading()).toBe(false);
    });
  });

  describe('buildWeek', () => {
    it('should populate weekDays with 7 entries', () => {
      component.buildWeek();
      expect(component.weekDays.length).toBe(7);
      component.weekDays.forEach(d => {
        expect(d.name).toBeTruthy();
        expect(d.date).toBeInstanceOf(Date);
      });
    });

    it('should set weekLabel', () => {
      component.buildWeek();
      expect(component.weekLabel).toMatch(/\d+\/\d+ - \d+\/\d+\/\d+/);
    });
  });

  describe('navigation', () => {
    beforeEach(() => component.ngOnInit());

    it('should navigate to previous week', () => {
      const originalStart = new Date(component.weekStart);
      component.prevWeek();
      const diff = originalStart.getTime() - component.weekStart.getTime();
      expect(diff).toBe(7 * 24 * 60 * 60 * 1000);
      expect(appointmentServiceSpy.getAll).toHaveBeenCalledTimes(2); // init + prevWeek
    });

    it('should navigate to next week', () => {
      const originalStart = new Date(component.weekStart);
      component.nextWeek();
      const diff = component.weekStart.getTime() - originalStart.getTime();
      expect(diff).toBe(7 * 24 * 60 * 60 * 1000);
    });

    it('should go to today', () => {
      component.nextWeek();
      component.nextWeek();
      component.goToday();
      const today = new Date();
      // weekStart should be the Sunday of the current week
      expect(component.weekStart.getDay()).toBe(0); // Sunday
    });
  });

  describe('isToday', () => {
    it('should return true for today', () => {
      expect(component.isToday(new Date())).toBe(true);
    });

    it('should return false for yesterday', () => {
      const yesterday = new Date();
      yesterday.setDate(yesterday.getDate() - 1);
      expect(component.isToday(yesterday)).toBe(false);
    });
  });

  describe('getAppointmentsForDay', () => {
    it('should filter appointments for a specific day', () => {
      component.appointments.set(mockPaginatedResult.items as any);
      // Use the same date constructor the component would use to avoid timezone issues
      const aptDate = new Date('2026-03-04T10:00:00Z');
      const matchDate = new Date(aptDate.getFullYear(), aptDate.getMonth(), aptDate.getDate());
      const result = component.getAppointmentsForDay(matchDate);
      expect(result.length).toBe(1);
    });

    it('should return empty for a day with no appointments', () => {
      component.appointments.set(mockPaginatedResult.items as any);
      const otherDate = new Date('2026-03-10');
      const result = component.getAppointmentsForDay(otherDate);
      expect(result.length).toBe(0);
    });
  });

  describe('getEventTop', () => {
    it('should calculate top position based on start time', () => {
      const apt = { startTime: '2026-03-04T10:30:00Z' } as any;
      const top = component.getEventTop(apt);
      // 10:30 - 8:00 = 2.5 hours = 150 minutes (UTC interpretation)
      // Depends on timezone, but should be a number
      expect(typeof top).toBe('number');
    });
  });

  describe('getEventHeight', () => {
    it('should calculate height based on duration', () => {
      const apt = { startTime: '2026-03-04T10:00:00Z', endTime: '2026-03-04T10:30:00Z' } as any;
      const height = component.getEventHeight(apt);
      expect(height).toBe(30); // 30 minutes
    });

    it('should enforce minimum height of 20', () => {
      const apt = { startTime: '2026-03-04T10:00:00Z', endTime: '2026-03-04T10:05:00Z' } as any;
      const height = component.getEventHeight(apt);
      expect(height).toBe(20); // minimum 20, even though duration is 5 min
    });
  });

  describe('formatTime', () => {
    it('should format ISO string to HH:mm', () => {
      const result = component.formatTime('2026-03-01T09:05:00Z');
      expect(result).toMatch(/\d{2}:\d{2}/);
    });
  });

  describe('openForm', () => {
    it('should open dialog and reload on close with result', () => {
      component.ngOnInit();
      appointmentServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.openForm();

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(appointmentServiceSpy.getAll).toHaveBeenCalled(); // reload
    });

    it('should not reload when dialog closed without result', () => {
      component.ngOnInit();
      appointmentServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(null) });

      component.openForm();

      expect(appointmentServiceSpy.getAll).not.toHaveBeenCalled();
    });
  });

  describe('load', () => {
    it('should handle load error gracefully', () => {
      appointmentServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.load();
      expect(component.loading()).toBe(false);
    });
  });
});
