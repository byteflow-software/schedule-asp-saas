import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { ConfirmDialogComponent } from './confirm-dialog.component';

describe('ConfirmDialogComponent', () => {
  let component: ConfirmDialogComponent;
  let dialogRefSpy: { close: jest.Mock };

  beforeEach(() => {
    dialogRefSpy = { close: jest.fn() };
    TestBed.configureTestingModule({
      imports: [ConfirmDialogComponent, NoopAnimationsModule],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: { title: 'Delete?', message: 'Are you sure?', confirmText: 'Delete', cancelText: 'No' } },
        { provide: MatDialogRef, useValue: dialogRefSpy },
      ],
    });
    const fixture = TestBed.createComponent(ConfirmDialogComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should inject dialog data', () => {
    expect(component.data.title).toBe('Delete?');
    expect(component.data.message).toBe('Are you sure?');
  });

  it('should have dialogRef', () => {
    expect(component.dialogRef).toBeTruthy();
  });
});
