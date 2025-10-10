import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PersonListComponent } from './person-list.component';
import { PersonService } from '../../services/person.service';
import { of, throwError } from 'rxjs';
import { Person } from '../../models/person.model';
import { provideRouter } from '@angular/router';

describe('PersonListComponent', () => {
  let component: PersonListComponent;
  let fixture: ComponentFixture<PersonListComponent>;
  let personServiceSpy: jasmine.SpyObj<PersonService>;

  const mockPersons: Person[] = [
    {
      id: 1,
      name: 'John Doe',
      age: 30,
      dateOfBirth: new Date('1994-01-15'),
      skills: ['JavaScript', 'TypeScript'],
      createdAt: new Date('2024-01-01')
    },
    {
      id: 2,
      name: 'Jane Smith',
      age: 25,
      dateOfBirth: new Date('1999-05-20'),
      skills: ['Angular', 'React'],
      createdAt: new Date('2024-01-02')
    }
  ];

  beforeEach(async () => {
    const personServiceMock = jasmine.createSpyObj('PersonService', [
      'getPersons',
      'searchPersons',
      'deletePerson'
    ]);

    await TestBed.configureTestingModule({
      imports: [PersonListComponent],
      providers: [
        { provide: PersonService, useValue: personServiceMock },
        provideRouter([])
      ]
    }).compileComponents();

    personServiceSpy = TestBed.inject(PersonService) as jasmine.SpyObj<PersonService>;
    fixture = TestBed.createComponent(PersonListComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    it('should load persons on initialization', () => {
      personServiceSpy.getPersons.and.returnValue(of(mockPersons));

      fixture.detectChanges(); // triggers ngOnInit

      expect(personServiceSpy.getPersons).toHaveBeenCalledWith(1, 10);
      expect(component.persons).toEqual(mockPersons);
      expect(component.filteredPersons).toEqual(mockPersons);
      expect(component.loading).toBe(false);
    });

    it('should handle error when loading persons fails', () => {
      const errorResponse = { status: 500, message: 'Server error' };
      personServiceSpy.getPersons.and.returnValue(throwError(() => errorResponse));

      spyOn(console, 'error');
      fixture.detectChanges();

      expect(component.errorMessage).toBe('Failed to load persons. Please try again.');
      expect(component.loading).toBe(false);
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('loadPersons', () => {
    it('should set loading to true while loading', () => {
      personServiceSpy.getPersons.and.returnValue(of(mockPersons));

      component.loadPersons();

      expect(component.loading).toBe(false); // After observable completes
      expect(personServiceSpy.getPersons).toHaveBeenCalled();
    });

    it('should clear error message when reloading', () => {
      personServiceSpy.getPersons.and.returnValue(of(mockPersons));
      component.errorMessage = 'Previous error';

      component.loadPersons();

      expect(component.errorMessage).toBe('');
    });

    it('should load persons with current page and size', () => {
      personServiceSpy.getPersons.and.returnValue(of(mockPersons));
      component.pageNumber = 2;
      component.pageSize = 5;

      component.loadPersons();

      expect(personServiceSpy.getPersons).toHaveBeenCalledWith(2, 5);
    });
  });

  describe('searchPersons', () => {
    beforeEach(() => {
      component.persons = mockPersons;
      component.filteredPersons = mockPersons;
    });

    it('should search persons by name when searchTerm is provided', () => {
      const searchResults = [mockPersons[0]];
      personServiceSpy.searchPersons.and.returnValue(of(searchResults));
      component.searchTerm = 'John';

      component.searchPersons();

      expect(personServiceSpy.searchPersons).toHaveBeenCalledWith('John');
      expect(component.filteredPersons).toEqual(searchResults);
      expect(component.loading).toBe(false);
    });

    it('should reset to all persons when searchTerm is empty', () => {
      component.searchTerm = '';
      component.filteredPersons = [mockPersons[0]];

      component.searchPersons();

      expect(personServiceSpy.searchPersons).not.toHaveBeenCalled();
      expect(component.filteredPersons).toEqual(mockPersons);
    });

    it('should reset to all persons when searchTerm is only whitespace', () => {
      component.searchTerm = '   ';
      component.filteredPersons = [mockPersons[0]];

      component.searchPersons();

      expect(personServiceSpy.searchPersons).not.toHaveBeenCalled();
      expect(component.filteredPersons).toEqual(mockPersons);
    });

    it('should handle search errors', () => {
      const errorResponse = { status: 500, message: 'Search failed' };
      personServiceSpy.searchPersons.and.returnValue(throwError(() => errorResponse));
      component.searchTerm = 'John';

      spyOn(console, 'error');
      component.searchPersons();

      expect(component.errorMessage).toBe('Search failed. Please try again.');
      expect(component.loading).toBe(false);
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('deletePerson', () => {
    it('should delete person when confirmed', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      personServiceSpy.deletePerson.and.returnValue(of(void 0));
      personServiceSpy.getPersons.and.returnValue(of(mockPersons));

      component.deletePerson(1, 'John Doe');

      expect(window.confirm).toHaveBeenCalledWith('Are you sure you want to delete John Doe?');
      expect(personServiceSpy.deletePerson).toHaveBeenCalledWith(1);
      expect(personServiceSpy.getPersons).toHaveBeenCalled();
    });

    it('should not delete person when not confirmed', () => {
      spyOn(window, 'confirm').and.returnValue(false);

      component.deletePerson(1, 'John Doe');

      expect(window.confirm).toHaveBeenCalled();
      expect(personServiceSpy.deletePerson).not.toHaveBeenCalled();
    });

    it('should handle delete errors', () => {
      const errorResponse = { status: 500, message: 'Delete failed' };
      spyOn(window, 'confirm').and.returnValue(true);
      personServiceSpy.deletePerson.and.returnValue(throwError(() => errorResponse));

      spyOn(console, 'error');
      component.deletePerson(1, 'John Doe');

      expect(component.errorMessage).toBe('Failed to delete person. Please try again.');
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('clearSearch', () => {
    it('should clear search term and reset filtered persons', () => {
      component.persons = mockPersons;
      component.searchTerm = 'John';
      component.filteredPersons = [mockPersons[0]];

      component.clearSearch();

      expect(component.searchTerm).toBe('');
      expect(component.filteredPersons).toEqual(mockPersons);
    });
  });

  describe('pagination', () => {
    beforeEach(() => {
      personServiceSpy.getPersons.and.returnValue(of(mockPersons));
    });

    describe('previousPage', () => {
      it('should decrement page number and reload when page > 1', () => {
        component.pageNumber = 2;

        component.previousPage();

        expect(component.pageNumber).toBe(1);
        expect(personServiceSpy.getPersons).toHaveBeenCalledWith(1, 10);
      });

      it('should not go below page 1', () => {
        component.pageNumber = 1;

        component.previousPage();

        expect(component.pageNumber).toBe(1);
        expect(personServiceSpy.getPersons).not.toHaveBeenCalled();
      });
    });

    describe('nextPage', () => {
      it('should increment page number and reload', () => {
        component.pageNumber = 1;

        component.nextPage();

        expect(component.pageNumber).toBe(2);
        expect(personServiceSpy.getPersons).toHaveBeenCalledWith(2, 10);
      });

      it('should allow incrementing to any page number', () => {
        component.pageNumber = 5;

        component.nextPage();

        expect(component.pageNumber).toBe(6);
        expect(personServiceSpy.getPersons).toHaveBeenCalledWith(6, 10);
      });
    });
  });

  describe('component properties', () => {
    it('should initialize with default values', () => {
      expect(component.persons).toEqual([]);
      expect(component.filteredPersons).toEqual([]);
      expect(component.searchTerm).toBe('');
      expect(component.pageNumber).toBe(1);
      expect(component.pageSize).toBe(10);
      expect(component.totalCount).toBe(0);
      expect(component.loading).toBe(false);
      expect(component.errorMessage).toBe('');
    });
  });
});
