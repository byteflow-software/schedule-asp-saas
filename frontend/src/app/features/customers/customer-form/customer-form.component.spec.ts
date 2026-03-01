import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { CustomerFormComponent } from './customer-form.component';
import { CustomerService } from '../../../core/services/customer.service';

describe('CustomerFormComponent', () => {
  let component: CustomerFormComponent;
  let fixture: ComponentFixture<CustomerFormComponent>;
  let customerServiceSpy: { create: jest.Mock; update: jest.Mock };
  let dialogRefSpy: { close: jest.Mock };
  let snackBarSpy: { open: jest.Mock };

  function setup(data: any = null) {
    customerServiceSpy = { create: jest.fn().mockReturnValue(of({})), update: jest.fn().mockReturnValue(of({})) };
    dialogRefSpy = { close: jest.fn() };
    snackBarSpy = { open: jest.fn() };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [CustomerFormComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: data },
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: CustomerService, useValue: customerServiceSpy },
      ],
    });

    fixture = TestBed.createComponent(CustomerFormComponent);
    component = fixture.componentInstance;
  }

  describe('new customer (no data)', () => {
    beforeEach(() => setup(null));

    it('should create', () => {
      expect(component).toBeTruthy();
    });

    it('should have form fields', () => {
      expect(component.form.controls.fullName).toBeDefined();
      expect(component.form.controls.email).toBeDefined();
      expect(component.form.controls.phone).toBeDefined();
      expect(component.form.controls.cpfCnpj).toBeDefined();
    });

    it('should require fullName, cpfCnpj, and email', () => {
      expect(component.form.invalid).toBe(true);
    });

    it('should be valid with required fields filled', () => {
      component.form.patchValue({ fullName: 'John', cpfCnpj: '123', email: 'j@t.com' });
      expect(component.form.valid).toBe(true);
    });

    it('should not call service when form is invalid', () => {
      component.save();
      expect(customerServiceSpy.create).not.toHaveBeenCalled();
    });

    it('should call create on save for new customer', () => {
      component.form.patchValue({ fullName: 'John', cpfCnpj: '123', email: 'j@t.com' });
      component.save();
      expect(customerServiceSpy.create).toHaveBeenCalled();
      expect(snackBarSpy.open).toHaveBeenCalledWith('Cliente criado com sucesso!', 'OK', expect.any(Object));
      expect(dialogRefSpy.close).toHaveBeenCalledWith(true);
    });

    it('should handle save error', () => {
      customerServiceSpy.create.mockReturnValue(throwError(() => new Error('fail')));
      component.form.patchValue({ fullName: 'John', cpfCnpj: '123', email: 'j@t.com' });
      component.save();
      expect(component.saving).toBe(false);
      expect(dialogRefSpy.close).not.toHaveBeenCalled();
    });
  });

  describe('edit customer (with data)', () => {
    const existingCustomer = { id: 'c1', fullName: 'John', cpfCnpj: '123', email: 'j@t.com', phone: '999' };

    beforeEach(() => setup(existingCustomer));

    it('should populate form with existing data', () => {
      expect(component.form.getRawValue().fullName).toBe('John');
      expect(component.form.getRawValue().email).toBe('j@t.com');
    });

    it('should call update on save for existing customer', () => {
      component.save();
      expect(customerServiceSpy.update).toHaveBeenCalledWith(expect.objectContaining({ id: 'c1' }));
      expect(snackBarSpy.open).toHaveBeenCalledWith('Cliente atualizado com sucesso!', 'OK', expect.any(Object));
      expect(dialogRefSpy.close).toHaveBeenCalledWith(true);
    });
  });
});
