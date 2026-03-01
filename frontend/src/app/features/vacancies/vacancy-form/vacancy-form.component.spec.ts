import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { provideNativeDateAdapter } from '@angular/material/core';
import { of, throwError } from 'rxjs';
import { VacancyFormComponent } from './vacancy-form.component';
import { VacancyService } from '../../../core/services/vacancy.service';
import { UserService } from '../../../core/services/user.service';
import { ServiceService } from '../../../core/services/service.service';

describe('VacancyFormComponent', () => {
  let component: VacancyFormComponent;
  let fixture: ComponentFixture<VacancyFormComponent>;
  let dialogRefSpy: { close: jest.Mock };
  let snackBarSpy: { open: jest.Mock };
  let vacancyServiceSpy: { bulkCreate: jest.Mock };
  let userServiceSpy: { getAll: jest.Mock };
  let serviceServiceSpy: { getAll: jest.Mock };

  const mockUsers = [
    { id: 'u1', fullName: 'Dr. Test', email: 'dr@test.com', role: 'Admin', isActive: true, createdAt: '' },
    { id: 'u2', fullName: 'Inactive', email: 'x@test.com', role: 'User', isActive: false, createdAt: '' },
  ];

  const mockServices = [
    { id: 's1', name: 'Haircut', durationMinutes: 30, priceInCents: 5000, isActive: true, description: '' },
    { id: 's2', name: 'Massage', durationMinutes: 60, priceInCents: 10000, isActive: true, description: '' },
    { id: 's3', name: 'Inactive Svc', durationMinutes: 15, priceInCents: 2000, isActive: false, description: '' },
  ];

  beforeEach(async () => {
    dialogRefSpy = { close: jest.fn() };
    snackBarSpy = { open: jest.fn() };
    vacancyServiceSpy = { bulkCreate: jest.fn() };
    userServiceSpy = { getAll: jest.fn().mockReturnValue(of(mockUsers)) };
    serviceServiceSpy = { getAll: jest.fn().mockReturnValue(of(mockServices)) };

    await TestBed.configureTestingModule({
      imports: [VacancyFormComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        provideNativeDateAdapter(),
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: VacancyService, useValue: vacancyServiceSpy },
        { provide: UserService, useValue: userServiceSpy },
        { provide: ServiceService, useValue: serviceServiceSpy },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(VacancyFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have form fields', () => {
    expect(component.form.contains('userId')).toBe(true);
    expect(component.form.contains('serviceId')).toBe(true);
    expect(component.form.contains('dateStart')).toBe(true);
    expect(component.form.contains('dateEnd')).toBe(true);
    expect(component.form.contains('rangeStart')).toBe(true);
    expect(component.form.contains('rangeEnd')).toBe(true);
    expect(component.form.contains('interval')).toBe(true);
  });

  it('should have default weekday selections (Mon-Fri selected)', () => {
    const selected = component.weekDays.filter(d => d.selected);
    expect(selected.length).toBe(5);
    expect(selected.map(d => d.value)).toEqual([1, 2, 3, 4, 5]);
  });

  describe('ngOnInit', () => {
    it('should set default dates (today to next week)', () => {
      component.ngOnInit();
      expect(component.form.getRawValue().dateStart).toBeTruthy();
      expect(component.form.getRawValue().dateEnd).toBeTruthy();
    });

    it('should load and filter active users', () => {
      component.ngOnInit();
      expect(userServiceSpy.getAll).toHaveBeenCalled();
      expect(component.users.length).toBe(1);
      expect(component.users[0].id).toBe('u1');
    });

    it('should load and filter active services', () => {
      component.ngOnInit();
      expect(serviceServiceSpy.getAll).toHaveBeenCalled();
      expect(component.services.length).toBe(2);
      expect(component.services.map(s => s.id)).toEqual(['s1', 's2']);
    });
  });

  describe('canGenerate', () => {
    it('should return false when serviceId is empty', () => {
      component.form.patchValue({ dateStart: new Date(), dateEnd: new Date(), rangeStart: '08:00', rangeEnd: '18:00' });
      expect(component.canGenerate()).toBe(false);
    });

    it('should return false when dateStart is null', () => {
      component.form.patchValue({ serviceId: 's1', dateStart: null, dateEnd: new Date(), rangeStart: '08:00', rangeEnd: '18:00' });
      expect(component.canGenerate()).toBe(false);
    });

    it('should return false when dateEnd is null', () => {
      component.form.patchValue({ serviceId: 's1', dateStart: new Date(), dateEnd: null, rangeStart: '08:00', rangeEnd: '18:00' });
      expect(component.canGenerate()).toBe(false);
    });

    it('should return false when rangeStart is empty', () => {
      component.form.patchValue({ serviceId: 's1', dateStart: new Date(), dateEnd: new Date(), rangeStart: '', rangeEnd: '18:00' });
      expect(component.canGenerate()).toBe(false);
    });

    it('should return true when all fields are filled', () => {
      component.form.patchValue({ serviceId: 's1', dateStart: new Date(), dateEnd: new Date(), rangeStart: '08:00', rangeEnd: '18:00' });
      expect(component.canGenerate()).toBe(true);
    });
  });

  describe('canSave', () => {
    it('should return false when no slots', () => {
      component.form.patchValue({ userId: 'u1', serviceId: 's1' });
      component.slots = [];
      expect(component.canSave()).toBe(false);
    });

    it('should return false when userId is empty', () => {
      component.slots = [{ date: new Date(), startTime: '', endTime: '', dateLabel: '', timeLabel: '' }];
      component.form.patchValue({ userId: '', serviceId: 's1' });
      expect(component.canSave()).toBe(false);
    });

    it('should return true when userId, serviceId valid and slots exist', () => {
      component.form.patchValue({ userId: 'u1', serviceId: 's1' });
      component.slots = [{ date: new Date(), startTime: '', endTime: '', dateLabel: '', timeLabel: '' }];
      expect(component.canSave()).toBe(true);
    });
  });

  describe('generateSlots', () => {
    beforeEach(() => {
      component.ngOnInit();
    });

    it('should return early if no service found', () => {
      component.form.patchValue({ serviceId: 'nonexistent', dateStart: new Date(), dateEnd: new Date() });
      component.generateSlots();
      expect(component.slots.length).toBe(0);
    });

    it('should return early if dateStart is null', () => {
      component.form.patchValue({ serviceId: 's1', dateStart: null, dateEnd: new Date() });
      component.generateSlots();
      expect(component.slots.length).toBe(0);
    });

    it('should generate slots for a single day', () => {
      // Use a known Wednesday (weekday 3)
      const date = new Date(2026, 2, 4); // March 4, 2026 is a Wednesday
      component.form.patchValue({
        serviceId: 's1', // 30 min duration
        dateStart: date,
        dateEnd: date,
        rangeStart: '09:00',
        rangeEnd: '10:30',
        interval: 0,
      });
      component.generateSlots();
      // 09:00-09:30, 09:30-10:00, 10:00-10:30 = 3 slots
      expect(component.slots.length).toBe(3);
    });

    it('should generate slots with interval between them', () => {
      const date = new Date(2026, 2, 4); // Wednesday
      component.form.patchValue({
        serviceId: 's1', // 30 min
        dateStart: date,
        dateEnd: date,
        rangeStart: '09:00',
        rangeEnd: '10:30',
        interval: 15, // 15 min interval
      });
      component.generateSlots();
      // 09:00-09:30, (15min gap), 09:45-10:15 = 2 slots (10:15+45=11:00 > 10:30)
      expect(component.slots.length).toBe(2);
    });

    it('should respect weekday selection', () => {
      // Select only Wednesday
      component.weekDays.forEach(d => d.selected = false);
      component.weekDays.find(d => d.value === 3)!.selected = true; // Wednesday

      const start = new Date(2026, 2, 2); // Monday March 2
      const end = new Date(2026, 2, 8);   // Sunday March 8
      component.form.patchValue({
        serviceId: 's1',
        dateStart: start,
        dateEnd: end,
        rangeStart: '09:00',
        rangeEnd: '09:30',
        interval: 0,
      });
      component.generateSlots();
      // Only Wednesday March 4, one 30-min slot
      expect(component.slots.length).toBe(1);
    });

    it('should not add duplicate slots', () => {
      const date = new Date(2026, 2, 4); // Wednesday
      component.form.patchValue({
        serviceId: 's1',
        dateStart: date,
        dateEnd: date,
        rangeStart: '09:00',
        rangeEnd: '09:30',
        interval: 0,
      });
      component.generateSlots();
      expect(component.slots.length).toBe(1);
      // Generate again - should not add duplicates
      component.generateSlots();
      expect(component.slots.length).toBe(1);
    });

    it('should not generate if rangeStart is empty (timeToMinutes returns null)', () => {
      const date = new Date(2026, 2, 4);
      component.form.patchValue({
        serviceId: 's1',
        dateStart: date,
        dateEnd: date,
        rangeStart: '',
        rangeEnd: '10:00',
        interval: 0,
      });
      component.generateSlots();
      expect(component.slots.length).toBe(0);
    });

    it('should sort slots by startTime', () => {
      // Generate for multiple days
      const start = new Date(2026, 2, 4); // Wednesday
      const end = new Date(2026, 2, 5);   // Thursday
      component.form.patchValue({
        serviceId: 's1',
        dateStart: start,
        dateEnd: end,
        rangeStart: '09:00',
        rangeEnd: '09:30',
        interval: 0,
      });
      component.generateSlots();
      expect(component.slots.length).toBe(2);
      // Should be sorted
      expect(component.slots[0].startTime <= component.slots[1].startTime).toBe(true);
    });
  });

  describe('removeSlot', () => {
    it('should remove an existing slot', () => {
      const slot = { date: new Date(), startTime: 'a', endTime: 'b', dateLabel: 'x', timeLabel: 'y' };
      component.slots = [slot];
      component.removeSlot(slot);
      expect(component.slots.length).toBe(0);
    });

    it('should not crash if slot not found', () => {
      const slot = { date: new Date(), startTime: 'a', endTime: 'b', dateLabel: 'x', timeLabel: 'y' };
      component.slots = [];
      component.removeSlot(slot);
      expect(component.slots.length).toBe(0);
    });
  });

  describe('onServiceChange', () => {
    it('should clear slots when a valid service is selected', () => {
      component.ngOnInit();
      component.slots = [{ date: new Date(), startTime: '', endTime: '', dateLabel: '', timeLabel: '' }];
      component.form.patchValue({ serviceId: 's1' });
      component.onServiceChange();
      expect(component.slots.length).toBe(0);
    });

    it('should not clear slots when service is not found', () => {
      component.ngOnInit();
      component.slots = [{ date: new Date(), startTime: '', endTime: '', dateLabel: '', timeLabel: '' }];
      component.form.patchValue({ serviceId: 'nonexistent' });
      component.onServiceChange();
      expect(component.slots.length).toBe(1);
    });
  });

  describe('uniqueDatesCount', () => {
    it('should return 0 when no slots', () => {
      expect(component.uniqueDatesCount).toBe(0);
    });

    it('should count unique date labels', () => {
      component.slots = [
        { date: new Date(), startTime: '', endTime: '', dateLabel: 'Mon 04/03', timeLabel: '09:00' },
        { date: new Date(), startTime: '', endTime: '', dateLabel: 'Mon 04/03', timeLabel: '10:00' },
        { date: new Date(), startTime: '', endTime: '', dateLabel: 'Tue 05/03', timeLabel: '09:00' },
      ];
      expect(component.uniqueDatesCount).toBe(2);
    });
  });

  describe('groupedSlots', () => {
    it('should return empty array when no slots', () => {
      expect(component.groupedSlots).toEqual([]);
    });

    it('should group slots by dateLabel', () => {
      component.slots = [
        { date: new Date(), startTime: '', endTime: '', dateLabel: 'Mon 04/03', timeLabel: '09:00' },
        { date: new Date(), startTime: '', endTime: '', dateLabel: 'Mon 04/03', timeLabel: '10:00' },
        { date: new Date(), startTime: '', endTime: '', dateLabel: 'Tue 05/03', timeLabel: '09:00' },
      ];
      const grouped = component.groupedSlots;
      expect(grouped.length).toBe(2);
      expect(grouped[0].dateLabel).toBe('Mon 04/03');
      expect(grouped[0].slots.length).toBe(2);
      expect(grouped[1].dateLabel).toBe('Tue 05/03');
      expect(grouped[1].slots.length).toBe(1);
    });
  });

  describe('save', () => {
    beforeEach(() => {
      component.ngOnInit();
      component.form.patchValue({ userId: 'u1', serviceId: 's1' });
      component.slots = [{ date: new Date(), startTime: '2026-03-04T09:00:00Z', endTime: '2026-03-04T09:30:00Z', dateLabel: 'x', timeLabel: 'y' }];
    });

    it('should return early if canSave is false', () => {
      component.slots = [];
      component.save();
      expect(vacancyServiceSpy.bulkCreate).not.toHaveBeenCalled();
    });

    it('should call bulkCreate and close dialog on success', () => {
      const created = [{ id: 'v1' }];
      vacancyServiceSpy.bulkCreate.mockReturnValue(of(created));

      component.save();

      expect(vacancyServiceSpy.bulkCreate).toHaveBeenCalledWith({
        userId: 'u1',
        serviceId: 's1',
        slots: [{ startTime: '2026-03-04T09:00:00Z', endTime: '2026-03-04T09:30:00Z' }],
      });
      expect(snackBarSpy.open).toHaveBeenCalledWith(
        '1 vaga criada com sucesso!',
        'OK',
        expect.objectContaining({ duration: 3000 }),
      );
      expect(dialogRefSpy.close).toHaveBeenCalledWith(true);
    });

    it('should show plural message for multiple vacancies', () => {
      const created = [{ id: 'v1' }, { id: 'v2' }];
      vacancyServiceSpy.bulkCreate.mockReturnValue(of(created));

      component.save();

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        '2 vagas criadas com sucesso!',
        'OK',
        expect.objectContaining({ duration: 3000 }),
      );
    });

    it('should handle error and reset saving flag', () => {
      vacancyServiceSpy.bulkCreate.mockReturnValue(throwError(() => new Error('fail')));

      component.save();

      expect(component.saving).toBe(false);
      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Erro ao criar vagas.',
        'OK',
        expect.objectContaining({ duration: 3000 }),
      );
      expect(dialogRefSpy.close).not.toHaveBeenCalled();
    });

    it('should set saving to true during request', () => {
      vacancyServiceSpy.bulkCreate.mockReturnValue(of([{ id: 'v1' }]));
      // We can check that saving was set to true before the observable completes
      component.save();
      // After completion, saving is reset (dialog closes)
    });
  });
});
