import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { ServiceFormComponent } from './service-form.component';
import { ServiceService } from '../../../core/services/service.service';

describe('ServiceFormComponent', () => {
  let component: ServiceFormComponent;
  let fixture: ComponentFixture<ServiceFormComponent>;
  let serviceServiceSpy: { create: jest.Mock; update: jest.Mock };
  let dialogRefSpy: { close: jest.Mock };
  let snackBarSpy: { open: jest.Mock };

  function setup(data: any = null) {
    serviceServiceSpy = { create: jest.fn().mockReturnValue(of({})), update: jest.fn().mockReturnValue(of({})) };
    dialogRefSpy = { close: jest.fn() };
    snackBarSpy = { open: jest.fn() };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [ServiceFormComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: ServiceService, useValue: serviceServiceSpy },
      ],
    });
    fixture = TestBed.createComponent(ServiceFormComponent);
    component = fixture.componentInstance;
  }

  describe('new service (no data)', () => {
    beforeEach(() => setup(null));

    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have form fields', () => {
      expect(component.form.controls.name).toBeDefined();
      expect(component.form.controls.description).toBeDefined();
      expect(component.form.controls.durationMinutes).toBeDefined();
      expect(component.form.controls.priceReais).toBeDefined();
      expect(component.form.controls.isActive).toBeDefined();
    });

    it('should have defaults for new service', () => {
      expect(component.form.getRawValue().durationMinutes).toBe(30);
      expect(component.form.getRawValue().priceReais).toBe(0);
      expect(component.form.getRawValue().isActive).toBe(true);
    });

    it('should require name', () => {
      component.form.controls.name.setValue('');
      expect(component.form.controls.name.hasError('required')).toBe(true);
    });

    it('should require minimum duration of 5', () => {
      component.form.controls.durationMinutes.setValue(2);
      expect(component.form.controls.durationMinutes.hasError('min')).toBe(true);
    });

    it('should not call service when form is invalid', () => {
      component.form.controls.name.setValue('');
      component.save();
      expect(serviceServiceSpy.create).not.toHaveBeenCalled();
    });

    it('should call create on save and convert price to cents', () => {
      component.form.patchValue({ name: 'Haircut', priceReais: 50.5 });
      component.save();
      expect(serviceServiceSpy.create).toHaveBeenCalledWith(expect.objectContaining({ priceInCents: 5050 }));
      expect(snackBarSpy.open).toHaveBeenCalledWith('Serviço criado com sucesso!', 'OK', expect.any(Object));
      expect(dialogRefSpy.close).toHaveBeenCalledWith(true);
    });

    it('should handle save error', () => {
      serviceServiceSpy.create.mockReturnValue(throwError(() => new Error('fail')));
      component.form.patchValue({ name: 'Haircut' });
      component.save();
      expect(component.saving).toBe(false);
      expect(dialogRefSpy.close).not.toHaveBeenCalled();
    });
  });

  describe('edit service (with data)', () => {
    const existingService = { id: 's1', name: 'Haircut', description: 'Basic', durationMinutes: 30, priceInCents: 5000, isActive: true };

    beforeEach(() => setup(existingService));

    it('should populate form with existing data', () => {
      expect(component.form.getRawValue().name).toBe('Haircut');
      expect(component.form.getRawValue().priceReais).toBe(50); // 5000 / 100
    });

    it('should call update on save for existing service', () => {
      component.save();
      expect(serviceServiceSpy.update).toHaveBeenCalledWith(expect.objectContaining({ id: 's1', priceInCents: 5000 }));
      expect(snackBarSpy.open).toHaveBeenCalledWith('Serviço atualizado com sucesso!', 'OK', expect.any(Object));
    });
  });
});
