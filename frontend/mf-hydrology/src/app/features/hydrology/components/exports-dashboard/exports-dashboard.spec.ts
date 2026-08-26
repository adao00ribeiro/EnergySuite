import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ExportsDashboard } from './exports-dashboard';

describe('ExportsDashboard', () => {
  let component: ExportsDashboard;
  let fixture: ComponentFixture<ExportsDashboard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExportsDashboard],
    }).compileComponents();

    fixture = TestBed.createComponent(ExportsDashboard);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
