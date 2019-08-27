import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AinetComponent } from './ainet.component';

describe('AinetComponent', () => {
  let component: AinetComponent;
  let fixture: ComponentFixture<AinetComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AinetComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AinetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
