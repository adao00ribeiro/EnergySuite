import { ComponentFixture, TestBed } from '@angular/core/testing';
import { describe, it, expect, beforeEach } from 'vitest';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';

try {
  TestBed.initTestEnvironment(BrowserDynamicTestingModule, platformBrowserDynamicTesting());
} catch (e) {}
import { TicketsListComponent } from './tickets-list';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('TicketsListComponent', () => {
  let component: TicketsListComponent;
  let fixture: ComponentFixture<TicketsListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketsListComponent, BrowserAnimationsModule]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(TicketsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load mock data on init', () => {
    expect(component.dataSource().length).toBeGreaterThan(0);
    expect(component.dataSource()[0].ticketRef).toBe('TKT-2023-001');
  });
});
