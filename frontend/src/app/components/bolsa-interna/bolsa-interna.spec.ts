import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BolsaInterna } from './bolsa-interna';

describe('BolsaInterna', () => {
  let component: BolsaInterna;
  let fixture: ComponentFixture<BolsaInterna>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BolsaInterna],
    }).compileComponents();

    fixture = TestBed.createComponent(BolsaInterna);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
