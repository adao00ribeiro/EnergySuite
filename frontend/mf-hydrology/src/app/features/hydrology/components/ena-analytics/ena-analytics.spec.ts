import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EnaAnalytics } from './ena-analytics';

describe('EnaAnalytics', () => {
  let component: EnaAnalytics;
  let fixture: ComponentFixture<EnaAnalytics>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EnaAnalytics],
    }).compileComponents();

    fixture = TestBed.createComponent(EnaAnalytics);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
