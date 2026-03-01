import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { SidebarComponent } from './sidebar.component';
import { AuthService } from '../../../core/services/auth.service';
import { signal } from '@angular/core';

describe('SidebarComponent', () => {
  let component: SidebarComponent;
  let fixture: ComponentFixture<SidebarComponent>;

  beforeEach(async () => {
    const mockUser = signal({
      fullName: 'John Doe',
      role: 'Admin',
      tenantId: '1',
      tokens: { accessToken: 'token', refreshToken: 'refresh' },
    });

    await TestBed.configureTestingModule({
      imports: [SidebarComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        provideRouter([]),
        {
          provide: AuthService,
          useValue: {
            currentUser: mockUser,
            isAdmin: signal(true),
            isAuthenticated: signal(true),
            logout: jest.fn(),
          },
        },
      ],
    }).compileComponents();
    fixture = TestBed.createComponent(SidebarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have navigation links', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const navItems = compiled.querySelectorAll('.nav-item');
    expect(navItems.length).toBeGreaterThan(0);

    const linkTexts = Array.from(navItems).map(el => el.textContent?.trim());
    expect(linkTexts.some(t => t?.includes('Dashboard'))).toBe(true);
    expect(linkTexts.some(t => t?.includes('Agendamentos'))).toBe(true);
    expect(linkTexts.some(t => t?.includes('Clientes'))).toBe(true);
  });
});
