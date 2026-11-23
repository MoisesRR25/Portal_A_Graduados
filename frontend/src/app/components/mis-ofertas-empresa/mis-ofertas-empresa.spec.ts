import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MisOfertasEmpresa } from './mis-ofertas-empresa';

describe('MisOfertasEmpresa', () => {
  let component: MisOfertasEmpresa;
  let fixture: ComponentFixture<MisOfertasEmpresa>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MisOfertasEmpresa],
    }).compileComponents();

    fixture = TestBed.createComponent(MisOfertasEmpresa);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
