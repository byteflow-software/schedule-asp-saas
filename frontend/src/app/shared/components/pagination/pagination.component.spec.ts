import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { PaginationComponent } from './pagination.component';

@Component({
  standalone: true,
  imports: [PaginationComponent],
  template: `
    <app-pagination
      [pageNumber]="page"
      [totalPages]="total"
      [totalCount]="count"
      [hasPreviousPage]="hasPrev"
      [hasNextPage]="hasNext"
      (pageChange)="onPage($event)" />
  `,
})
class TestHostComponent {
  page = 2;
  total = 5;
  count = 50;
  hasPrev = true;
  hasNext = true;
  lastPage = 0;
  onPage(p: number) { this.lastPage = p; }
}

describe('PaginationComponent', () => {
  let fixture: ComponentFixture<TestHostComponent>;
  let host: TestHostComponent;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [TestHostComponent, NoopAnimationsModule],
    });
    fixture = TestBed.createComponent(TestHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    const pagination = fixture.debugElement.children[0].componentInstance as PaginationComponent;
    expect(pagination).toBeTruthy();
  });

  it('should display page info', () => {
    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Página 2 de 5');
    expect(text).toContain('50 itens');
  });

  it('should emit previous page on click', () => {
    const buttons = fixture.nativeElement.querySelectorAll('button');
    buttons[0].click();
    expect(host.lastPage).toBe(1); // pageNumber - 1
  });

  it('should emit next page on click', () => {
    const buttons = fixture.nativeElement.querySelectorAll('button');
    buttons[1].click();
    expect(host.lastPage).toBe(3); // pageNumber + 1
  });

  it('should disable previous button when hasPreviousPage is false', () => {
    host.hasPrev = false;
    fixture.detectChanges();
    const buttons = fixture.nativeElement.querySelectorAll('button');
    expect(buttons[0].disabled).toBe(true);
  });

  it('should disable next button when hasNextPage is false', () => {
    host.hasNext = false;
    fixture.detectChanges();
    const buttons = fixture.nativeElement.querySelectorAll('button');
    expect(buttons[1].disabled).toBe(true);
  });
});
