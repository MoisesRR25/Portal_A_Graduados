import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BuscarPerfiles } from './buscar-perfiles';

describe('BuscarPerfiles', () => {
  let component: BuscarPerfiles;
  let fixture: ComponentFixture<BuscarPerfiles>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BuscarPerfiles],
    }).compileComponents();

    fixture = TestBed.createComponent(BuscarPerfiles);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
