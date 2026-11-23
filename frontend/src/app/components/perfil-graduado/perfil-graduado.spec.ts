import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PerfilGraduado } from './perfil-graduado';

describe('PerfilGraduado', () => {
  let component: PerfilGraduado;
  let fixture: ComponentFixture<PerfilGraduado>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PerfilGraduado],
    }).compileComponents();

    fixture = TestBed.createComponent(PerfilGraduado);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
