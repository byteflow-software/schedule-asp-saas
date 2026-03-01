import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { of, throwError } from 'rxjs';
import { UserListComponent } from './user-list.component';
import { UserService } from '../../../core/services/user.service';

describe('UserListComponent', () => {
  let component: UserListComponent;
  let fixture: ComponentFixture<UserListComponent>;
  let userServiceSpy: { getAll: jest.Mock; deactivate: jest.Mock };
  let dialogSpy: { open: jest.Mock };
  let snackBarSpy: { open: jest.Mock };

  const mockUsers = [
    { id: '1', fullName: 'John Doe', email: 'john@test.com', role: 'Admin', isActive: true, createdAt: '2024-01-01' },
  ];

  beforeEach(async () => {
    userServiceSpy = {
      getAll: jest.fn().mockReturnValue(of(mockUsers)),
      deactivate: jest.fn().mockReturnValue(of(void 0)),
    };
    dialogSpy = { open: jest.fn(() => ({ afterClosed: () => of(true) })) };
    snackBarSpy = { open: jest.fn() };

    await TestBed.configureTestingModule({
      imports: [UserListComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: UserService, useValue: userServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(UserListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should load users', () => {
      component.ngOnInit();
      expect(userServiceSpy.getAll).toHaveBeenCalled();
      expect(component.users()).toEqual(mockUsers);
      expect(component.loading()).toBe(false);
    });
  });

  describe('load', () => {
    it('should handle error and show snackbar', () => {
      userServiceSpy.getAll.mockReturnValue(throwError(() => new Error('fail')));
      component.load();
      expect(component.loading()).toBe(false);
      expect(snackBarSpy.open).toHaveBeenCalledWith('Erro ao carregar equipe.', 'OK', expect.any(Object));
    });
  });

  describe('openForm', () => {
    it('should open dialog and reload on close with result', () => {
      component.ngOnInit();
      userServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });

      component.openForm();

      expect(dialogSpy.open).toHaveBeenCalled();
      expect(userServiceSpy.getAll).toHaveBeenCalled();
    });

    it('should not reload when dialog closed without result', () => {
      component.ngOnInit();
      userServiceSpy.getAll.mockClear();
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(null) });

      component.openForm();

      expect(userServiceSpy.getAll).not.toHaveBeenCalled();
    });
  });

  describe('confirmDeactivate', () => {
    const user = { id: '1', fullName: 'John Doe' } as any;

    it('should deactivate and show success on confirm', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(true) });
      component.ngOnInit();
      userServiceSpy.getAll.mockClear();

      component.confirmDeactivate(user);

      expect(userServiceSpy.deactivate).toHaveBeenCalledWith('1');
      expect(snackBarSpy.open).toHaveBeenCalledWith('Membro desativado com sucesso.', 'OK', expect.any(Object));
      expect(userServiceSpy.getAll).toHaveBeenCalled();
    });

    it('should not deactivate when dialog is dismissed', () => {
      dialogSpy.open.mockReturnValue({ afterClosed: () => of(false) });

      component.confirmDeactivate(user);

      expect(userServiceSpy.deactivate).not.toHaveBeenCalled();
    });
  });
});
