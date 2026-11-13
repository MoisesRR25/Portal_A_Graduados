import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RegistroGraduado } from './registro-graduado';

describe('RegistroGraduado', () => {
  let component: RegistroGraduado;
  let fixture: ComponentFixture<RegistroGraduado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegistroGraduado],
    }).compileComponents();

    fixture = TestBed.createComponent(RegistroGraduado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
