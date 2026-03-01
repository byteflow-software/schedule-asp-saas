import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { SettingsComponent } from './settings.component';

describe('SettingsComponent', () => {
  it('should create', () => {
    TestBed.configureTestingModule({
      imports: [SettingsComponent, NoopAnimationsModule],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    const fixture = TestBed.createComponent(SettingsComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
