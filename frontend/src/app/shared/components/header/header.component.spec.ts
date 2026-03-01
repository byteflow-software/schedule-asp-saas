import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { HeaderComponent } from './header.component';
import { AuthService } from '../../../core/services/auth.service';
import { signal } from '@angular/core';

describe('HeaderComponent', () => {
  let component: HeaderComponent;
  let fixture: ComponentFixture<HeaderComponent>;

  function createComponent(user: any) {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [HeaderComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            currentUser: signal(user),
            isAdmin: signal(true),
            logout: jest.fn(),
          },
        },
      ],
    });
    fixture = TestBed.createComponent(HeaderComponent);
    component = fixture.componentInstance;
  }

  it('should create', () => {
    createComponent({ fullName: 'John Doe', role: 'Admin' });
    expect(component).toBeTruthy();
  });

  describe('firstName', () => {
    it('should return first name', () => {
      createComponent({ fullName: 'John Doe', role: 'Admin' });
      expect(component.firstName()).toBe('John');
    });

    it('should return empty string when no user', () => {
      createComponent(null);
      expect(component.firstName()).toBe('');
    });
  });

  describe('initials', () => {
    it('should return first and last initials for two-word name', () => {
      createComponent({ fullName: 'John Doe', role: 'Admin' });
      expect(component.initials()).toBe('JD');
    });

    it('should return first and last initials for multi-word name', () => {
      createComponent({ fullName: 'John Middle Doe', role: 'Admin' });
      expect(component.initials()).toBe('JD');
    });

    it('should return single initial for single-word name', () => {
      createComponent({ fullName: 'John', role: 'Admin' });
      expect(component.initials()).toBe('J');
    });

    it('should return empty string when no user', () => {
      createComponent(null);
      expect(component.initials()).toBe('');
    });
  });
});
