import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CustomScenarios } from './custom-scenarios';

describe('CustomScenarios', () => {
  let component: CustomScenarios;
  let fixture: ComponentFixture<CustomScenarios>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CustomScenarios],
    }).compileComponents();

    fixture = TestBed.createComponent(CustomScenarios);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
