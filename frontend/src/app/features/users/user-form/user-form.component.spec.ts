import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { UserFormComponent } from './user-form.component';
import { UserService } from '../../../core/services/user.service';

describe('UserFormComponent', () => {
  let component: UserFormComponent;
  let fixture: ComponentFixture<UserFormComponent>;
  let dialogRefSpy: { close: jest.Mock };
  let snackBarSpy: { open: jest.Mock };
  let userServiceSpy: { create: jest.Mock };

  beforeEach(async () => {
    dialogRefSpy = { close: jest.fn() };
    snackBarSpy = { open: jest.fn() };
    userServiceSpy = { create: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [UserFormComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: UserService, useValue: userServiceSpy },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(UserFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have form fields', () => {
    expect(component.form.contains('fullName')).toBe(true);
    expect(component.form.contains('email')).toBe(true);
    expect(component.form.contains('password')).toBe(true);
    expect(component.form.contains('role')).toBe(true);
  });

  it('should default role to Staff', () => {
    expect(component.form.getRawValue().role).toBe('Staff');
  });

  it('should start with saving false', () => {
    expect(component.saving).toBe(false);
  });

  describe('save', () => {
    it('should not call service when form is invalid', () => {
      component.save();
      expect(userServiceSpy.create).not.toHaveBeenCalled();
    });

    it('should call create and close dialog on success', () => {
      userServiceSpy.create.mockReturnValue(of({}));
      component.form.patchValue({ fullName: 'John', email: 'j@t.com', password: 'pass123456' });

      component.save();

      expect(userServiceSpy.create).toHaveBeenCalledWith(expect.objectContaining({ fullName: 'John', email: 'j@t.com', role: 'Staff' }));
      expect(snackBarSpy.open).toHaveBeenCalledWith('Membro criado.', 'OK', expect.any(Object));
      expect(dialogRefSpy.close).toHaveBeenCalledWith(true);
    });

    it('should reset saving on error', () => {
      userServiceSpy.create.mockReturnValue(throwError(() => new Error('fail')));
      component.form.patchValue({ fullName: 'John', email: 'j@t.com', password: 'pass123456' });

      component.save();

      expect(component.saving).toBe(false);
      expect(dialogRefSpy.close).not.toHaveBeenCalled();
    });
  });
});
