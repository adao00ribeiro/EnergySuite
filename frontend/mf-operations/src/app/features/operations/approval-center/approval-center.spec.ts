import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BrowserDynamicTestingModule, platformBrowserDynamicTesting } from '@angular/platform-browser-dynamic/testing';
import { ApprovalCenterComponent } from './approval-center';

try {
  TestBed.initTestEnvironment(BrowserDynamicTestingModule, platformBrowserDynamicTesting());
} catch (e) {}
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { describe, it, expect, beforeEach, vi } from 'vitest';

describe('ApprovalCenterComponent', () => {
  let component: ApprovalCenterComponent;
  let fixture: ComponentFixture<ApprovalCenterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ApprovalCenterComponent, BrowserAnimationsModule]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ApprovalCenterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have approve and reject methods', () => {
    const consoleSpy = vi.spyOn(console, 'log');
    component.approve('123');
    expect(consoleSpy).toHaveBeenCalledWith('Approve', '123');

    component.reject('123');
    expect(consoleSpy).toHaveBeenCalledWith('Reject', '123');
  });
});
