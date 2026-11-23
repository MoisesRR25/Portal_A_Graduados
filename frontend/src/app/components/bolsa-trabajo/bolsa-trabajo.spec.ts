import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BolsaTrabajo } from './bolsa-trabajo';

describe('BolsaTrabajo', () => {
  let component: BolsaTrabajo;
  let fixture: ComponentFixture<BolsaTrabajo>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BolsaTrabajo],
    }).compileComponents();

    fixture = TestBed.createComponent(BolsaTrabajo);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
