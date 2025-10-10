import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PersonFormComponent } from './person-form.component';
import { PersonService } from '../../services/person.service';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Person } from '../../models/person.model';
import { provideRouter } from '@angular/router';

describe('PersonFormComponent', () => {
  let component: PersonFormComponent;
  let fixture: ComponentFixture<PersonFormComponent>;
  let personServiceSpy: jasmine.SpyObj<PersonService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let activatedRoute: ActivatedRoute;

  const mockPerson: Person = {
    id: 1,
    name: 'John Doe',
    age: 30,
    dateOfBirth: new Date('1994-01-15'),
    skills: ['JavaScript', 'TypeScript'],
    createdAt: new Date('2024-01-01')
  };

  beforeEach(async () => {
    const personServiceMock = jasmine.createSpyObj('PersonService', [
      'getPersonById',
      'createPerson',
      'updatePerson'
    ]);
    const routerMock = jasmine.createSpyObj('Router', ['navigate']);

    const activatedRouteMock = {
      params: of({})
    };

    await TestBed.configureTestingModule({
      imports: [PersonFormComponent],
      providers: [
        { provide: PersonService, useValue: personServiceMock },
        { provide: Router, useValue: routerMock },
        { provide: ActivatedRoute, useValue: activatedRouteMock }
      ]
    }).compileComponents();

    personServiceSpy = TestBed.inject(PersonService) as jasmine.SpyObj<PersonService>;
    routerSpy = TestBed.inject(Router) as jasmine.SpyObj<Router>;
    activatedRoute = TestBed.inject(ActivatedRoute);
    fixture = TestBed.createComponent(PersonFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Form Initialization', () => {
    it('should initialize form with empty values for create mode', () => {
      fixture.detectChanges();

      expect(component.personForm).toBeDefined();
      expect(component.personForm.get('name')?.value).toBe('');
      expect(component.personForm.get('age')?.value).toBe('');
      expect(component.personForm.get('dateOfBirth')?.value).toBe('');
      expect(component.skills.length).toBe(0);
    });

    it('should initialize in create mode by default', () => {
      fixture.detectChanges();

      expect(component.isEditMode).toBe(false);
      expect(component.personId).toBeNull();
    });

    it('should set edit mode when id parameter is present', () => {
      activatedRoute.params = of({ id: '1' });
      personServiceSpy.getPersonById.and.returnValue(of(mockPerson));

      fixture.detectChanges();

      expect(component.isEditMode).toBe(true);
      expect(component.personId).toBe(1);
    });
  });

  describe('Form Validation', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should have name as required', () => {
      const nameControl = component.personForm.get('name');
      nameControl?.setValue('');

      expect(nameControl?.hasError('required')).toBe(true);
      expect(nameControl?.valid).toBe(false);
    });

    it('should validate name max length', () => {
      const nameControl = component.personForm.get('name');
      const longName = 'a'.repeat(201);
      nameControl?.setValue(longName);

      expect(nameControl?.hasError('maxlength')).toBe(true);
    });

    it('should have age as required', () => {
      const ageControl = component.personForm.get('age');
      ageControl?.setValue('');

      expect(ageControl?.hasError('required')).toBe(true);
    });

    it('should validate age minimum value', () => {
      const ageControl = component.personForm.get('age');
      ageControl?.setValue(-1);

      expect(ageControl?.hasError('min')).toBe(true);
    });

    it('should validate age maximum value', () => {
      const ageControl = component.personForm.get('age');
      ageControl?.setValue(151);

      expect(ageControl?.hasError('max')).toBe(true);
    });

    it('should have dateOfBirth as required', () => {
      const dobControl = component.personForm.get('dateOfBirth');
      dobControl?.setValue('');

      expect(dobControl?.hasError('required')).toBe(true);
    });

    it('should be invalid when required fields are empty', () => {
      expect(component.personForm.valid).toBe(false);
    });

    it('should be valid when all required fields are filled correctly', () => {
      component.personForm.patchValue({
        name: 'Test Person',
        age: 25,
        dateOfBirth: '2000-01-01'
      });

      expect(component.personForm.valid).toBe(true);
    });
  });

  describe('Load Person (Edit Mode)', () => {
    it('should load person data in edit mode', () => {
      activatedRoute.params = of({ id: '1' });
      personServiceSpy.getPersonById.and.returnValue(of(mockPerson));

      fixture.detectChanges();

      expect(personServiceSpy.getPersonById).toHaveBeenCalledWith(1);
      expect(component.personForm.get('name')?.value).toBe('John Doe');
      expect(component.personForm.get('age')?.value).toBe(30);
      expect(component.skills.length).toBe(2);
      expect(component.skills.at(0).value).toBe('JavaScript');
      expect(component.skills.at(1).value).toBe('TypeScript');
    });

    it('should handle error when loading person fails', () => {
      activatedRoute.params = of({ id: '1' });
      const errorResponse = { status: 404, message: 'Not found' };
      personServiceSpy.getPersonById.and.returnValue(throwError(() => errorResponse));

      spyOn(console, 'error');
      fixture.detectChanges();

      expect(component.errorMessage).toBe('Failed to load person data.');
      expect(component.loading).toBe(false);
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('Skills Management', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should add a skill', () => {
      component.newSkill = 'Angular';

      component.addSkill();

      expect(component.skills.length).toBe(1);
      expect(component.skills.at(0).value).toBe('Angular');
      expect(component.newSkill).toBe('');
    });

    it('should not add empty skill', () => {
      component.newSkill = '';

      component.addSkill();

      expect(component.skills.length).toBe(0);
    });

    it('should not add skill with only whitespace', () => {
      component.newSkill = '   ';

      component.addSkill();

      expect(component.skills.length).toBe(0);
    });

    it('should trim skill before adding', () => {
      component.newSkill = '  Angular  ';

      component.addSkill();

      expect(component.skills.at(0).value).toBe('Angular');
    });

    it('should remove a skill by index', () => {
      component.newSkill = 'JavaScript';
      component.addSkill();
      component.newSkill = 'TypeScript';
      component.addSkill();

      component.removeSkill(0);

      expect(component.skills.length).toBe(1);
      expect(component.skills.at(0).value).toBe('TypeScript');
    });

    it('should allow adding multiple skills', () => {
      component.newSkill = 'JavaScript';
      component.addSkill();
      component.newSkill = 'TypeScript';
      component.addSkill();
      component.newSkill = 'Angular';
      component.addSkill();

      expect(component.skills.length).toBe(3);
    });
  });

  describe('Form Submission - Create Mode', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should create person when form is valid', () => {
      const createdPerson: Person = {
        id: 1,
        name: 'New Person',
        age: 25,
        dateOfBirth: new Date('2000-01-01'),
        skills: ['Angular'],
        createdAt: new Date(),
        updatedAt: undefined
      };

      component.personForm.patchValue({
        name: 'New Person',
        age: 25,
        dateOfBirth: '2000-01-01'
      });
      component.newSkill = 'Angular';
      component.addSkill();

      personServiceSpy.createPerson.and.returnValue(of(createdPerson));

      component.onSubmit();

      expect(personServiceSpy.createPerson).toHaveBeenCalled();
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/persons']);
    });

    it('should not submit when form is invalid', () => {
      component.personForm.patchValue({
        name: '',
        age: '',
        dateOfBirth: ''
      });

      component.onSubmit();

      expect(personServiceSpy.createPerson).not.toHaveBeenCalled();
      expect(component.personForm.get('name')?.touched).toBe(true);
      expect(component.personForm.get('age')?.touched).toBe(true);
      expect(component.personForm.get('dateOfBirth')?.touched).toBe(true);
    });

    it('should handle create error', () => {
      const errorResponse = { status: 400, message: 'Validation failed' };
      component.personForm.patchValue({
        name: 'Test Person',
        age: 25,
        dateOfBirth: '2000-01-01'
      });

      personServiceSpy.createPerson.and.returnValue(throwError(() => errorResponse));
      spyOn(console, 'error');

      component.onSubmit();

      expect(component.errorMessage).toBe('Failed to create person. Please try again.');
      expect(component.loading).toBe(false);
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('Form Submission - Edit Mode', () => {
    beforeEach(() => {
      component.isEditMode = true;
      component.personId = 1;
      fixture.detectChanges();
    });

    it('should update person when form is valid in edit mode', () => {
      component.personForm.patchValue({
        name: 'Updated Person',
        age: 31,
        dateOfBirth: '1993-01-01'
      });

      personServiceSpy.updatePerson.and.returnValue(of(void 0));

      component.onSubmit();

      expect(personServiceSpy.updatePerson).toHaveBeenCalledWith(1, jasmine.any(Object));
      expect(routerSpy.navigate).toHaveBeenCalledWith(['/persons']);
    });

    it('should handle update error', () => {
      const errorResponse = { status: 404, message: 'Not found' };
      component.personForm.patchValue({
        name: 'Updated Person',
        age: 31,
        dateOfBirth: '1993-01-01'
      });

      personServiceSpy.updatePerson.and.returnValue(throwError(() => errorResponse));
      spyOn(console, 'error');

      component.onSubmit();

      expect(component.errorMessage).toBe('Failed to update person. Please try again.');
      expect(component.loading).toBe(false);
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('Cancel', () => {
    it('should navigate to persons list when cancel is clicked', () => {
      fixture.detectChanges();

      component.cancel();

      expect(routerSpy.navigate).toHaveBeenCalledWith(['/persons']);
    });
  });

  describe('Convenience Getters', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should provide name control getter', () => {
      const nameControl = component.name;
      expect(nameControl).toBe(component.personForm.get('name'));
    });

    it('should provide age control getter', () => {
      const ageControl = component.age;
      expect(ageControl).toBe(component.personForm.get('age'));
    });

    it('should provide dateOfBirth control getter', () => {
      const dobControl = component.dateOfBirth;
      expect(dobControl).toBe(component.personForm.get('dateOfBirth'));
    });

    it('should provide skills FormArray getter', () => {
      const skillsArray = component.skills;
      expect(skillsArray).toBeDefined();
      expect(skillsArray.length).toBe(0);
    });
  });

  describe('Date Formatting', () => {
    it('should format date correctly for input field', () => {
      const date = new Date('1994-01-15');
      const formatted = (component as any).formatDateForInput(date);

      expect(formatted).toBe('1994-01-15');
    });

    it('should handle single digit months and days', () => {
      const date = new Date('2000-03-05');
      const formatted = (component as any).formatDateForInput(date);

      expect(formatted).toBe('2000-03-05');
    });
  });

  describe('Component State', () => {
    it('should initialize with default state', () => {
      expect(component.isEditMode).toBe(false);
      expect(component.personId).toBeNull();
      expect(component.loading).toBe(false);
      expect(component.errorMessage).toBe('');
      expect(component.newSkill).toBe('');
    });

    it('should set loading state during create operation', () => {
      fixture.detectChanges();
      component.personForm.patchValue({
        name: 'Test',
        age: 25,
        dateOfBirth: '2000-01-01'
      });

      const createdPerson: Person = { ...mockPerson };
      personServiceSpy.createPerson.and.returnValue(of(createdPerson));

      component.onSubmit();

      // Loading should be false after completion
      expect(component.loading).toBe(false);
    });
  });
});
