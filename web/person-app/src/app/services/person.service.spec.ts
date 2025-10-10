import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { PersonService } from './person.service';
import { Person, CreatePersonDto, UpdatePersonDto } from '../models/person.model';

describe('PersonService', () => {
  let service: PersonService;
  let httpMock: HttpTestingController;
  const apiUrl = '/api/persons';

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [PersonService]
    });
    service = TestBed.inject(PersonService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getPersons', () => {
    it('should return an array of persons with default pagination', () => {
      const mockPersons: Person[] = [
        {
          id: 1,
          name: 'John Doe',
          age: 30,
          dateOfBirth: new Date('1994-01-15'),
          skills: ['JavaScript', 'TypeScript'],
          createdAt: new Date(),
          updatedAt: undefined
        },
        {
          id: 2,
          name: 'Jane Smith',
          age: 25,
          dateOfBirth: new Date('1999-05-20'),
          skills: ['Angular', 'React'],
          createdAt: new Date(),
          updatedAt: undefined
        }
      ];

      service.getPersons().subscribe(persons => {
        expect(persons).toEqual(mockPersons);
        expect(persons.length).toBe(2);
      });

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=10`);
      expect(req.request.method).toBe('GET');
      req.flush(mockPersons);
    });

    it('should return persons with custom pagination parameters', () => {
      const mockPersons: Person[] = [];

      service.getPersons(2, 5).subscribe(persons => {
        expect(persons).toEqual(mockPersons);
      });

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=2&pageSize=5`);
      expect(req.request.method).toBe('GET');
      req.flush(mockPersons);
    });

    it('should handle empty array response', () => {
      service.getPersons().subscribe(persons => {
        expect(persons).toEqual([]);
        expect(persons.length).toBe(0);
      });

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=10`);
      req.flush([]);
    });
  });

  describe('getPersonById', () => {
    it('should return a person by id', () => {
      const mockPerson: Person = {
        id: 1,
        name: 'John Doe',
        age: 30,
        dateOfBirth: new Date('1994-01-15'),
        skills: ['JavaScript', 'TypeScript'],
        createdAt: new Date()      };

      service.getPersonById(1).subscribe(person => {
        expect(person).toEqual(mockPerson);
        expect(person.id).toBe(1);
        expect(person.name).toBe('John Doe');
      });

      const req = httpMock.expectOne(`${apiUrl}/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockPerson);
    });

    it('should handle 404 error when person not found', () => {
      const errorMessage = 'Person not found';

      service.getPersonById(999).subscribe(
        () => fail('should have failed with 404 error'),
        (error) => {
          expect(error.status).toBe(404);
        }
      );

      const req = httpMock.expectOne(`${apiUrl}/999`);
      req.flush(errorMessage, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('createPerson', () => {
    it('should create a new person and return it', () => {
      const createDto: CreatePersonDto = {
        name: 'New Person',
        age: 28,
        dateOfBirth: new Date('1996-03-10'),
        skills: ['Angular', 'TypeScript']
      };

      const mockResponse: Person = {
        id: 3,
        ...createDto,
        createdAt: new Date()      };

      service.createPerson(createDto).subscribe(person => {
        expect(person).toEqual(mockResponse);
        expect(person.id).toBe(3);
        expect(person.name).toBe(createDto.name);
      });

      const req = httpMock.expectOne(apiUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(createDto);
      expect(req.request.headers.get('Content-Type')).toBe('application/json');
      req.flush(mockResponse);
    });

    it('should handle validation errors', () => {
      const invalidDto: CreatePersonDto = {
        name: '',
        age: -5,
        dateOfBirth: new Date('2000-01-01'),
        skills: []
      };

      service.createPerson(invalidDto).subscribe(
        () => fail('should have failed with validation error'),
        (error) => {
          expect(error.status).toBe(400);
        }
      );

      const req = httpMock.expectOne(apiUrl);
      req.flush({ message: 'Validation failed' }, { status: 400, statusText: 'Bad Request' });
    });
  });

  describe('updatePerson', () => {
    it('should update an existing person', () => {
      const updateDto: UpdatePersonDto = {
        name: 'Updated Name',
        age: 31,
        dateOfBirth: new Date('1993-01-15'),
        skills: ['JavaScript', 'TypeScript', 'Angular']
      };

      service.updatePerson(1, updateDto).subscribe(() => {
        expect(true).toBe(true);
      });

      const req = httpMock.expectOne(`${apiUrl}/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(updateDto);
      expect(req.request.headers.get('Content-Type')).toBe('application/json');
      req.flush(null);
    });

    it('should handle 404 when updating non-existent person', () => {
      const updateDto: UpdatePersonDto = {
        name: 'Updated Name',
        age: 31,
        dateOfBirth: new Date('1993-01-15'),
        skills: ['JavaScript']
      };

      service.updatePerson(999, updateDto).subscribe(
        () => fail('should have failed with 404 error'),
        (error) => {
          expect(error.status).toBe(404);
        }
      );

      const req = httpMock.expectOne(`${apiUrl}/999`);
      req.flush({ message: 'Person not found' }, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('deletePerson', () => {
    it('should delete a person by id', () => {
      service.deletePerson(1).subscribe(() => {
        expect(true).toBe(true);
      });

      const req = httpMock.expectOne(`${apiUrl}/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });

    it('should handle 404 when deleting non-existent person', () => {
      service.deletePerson(999).subscribe(
        () => fail('should have failed with 404 error'),
        (error) => {
          expect(error.status).toBe(404);
        }
      );

      const req = httpMock.expectOne(`${apiUrl}/999`);
      req.flush({ message: 'Person not found' }, { status: 404, statusText: 'Not Found' });
    });
  });

  describe('searchPersons', () => {
    it('should search persons by name', () => {
      const mockPersons: Person[] = [
        {
          id: 1,
          name: 'John Doe',
          age: 30,
          dateOfBirth: new Date('1994-01-15'),
          skills: ['JavaScript'],
          createdAt: new Date(),
          updatedAt: undefined
        }
      ];

      service.searchPersons('John').subscribe(persons => {
        expect(persons).toEqual(mockPersons);
        expect(persons.length).toBe(1);
      });

      const req = httpMock.expectOne(`${apiUrl}/search?name=John`);
      expect(req.request.method).toBe('GET');
      req.flush(mockPersons);
    });

    it('should return empty array when no matches found', () => {
      service.searchPersons('NonExistent').subscribe(persons => {
        expect(persons).toEqual([]);
        expect(persons.length).toBe(0);
      });

      const req = httpMock.expectOne(`${apiUrl}/search?name=NonExistent`);
      req.flush([]);
    });

    it('should handle special characters in search query', () => {
      const searchTerm = 'O\'Brien';

      service.searchPersons(searchTerm).subscribe();

      const req = httpMock.expectOne(`${apiUrl}/search?name=O'Brien`);
      expect(req.request.method).toBe('GET');
      req.flush([]);
    });
  });

  describe('error handling', () => {
    it('should handle network errors', () => {
      const errorEvent = new ProgressEvent('error');

      service.getPersons().subscribe(
        () => fail('should have failed with network error'),
        (error) => {
          expect(error.error).toBe(errorEvent);
        }
      );

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=10`);
      req.error(errorEvent);
    });

    it('should handle server errors (500)', () => {
      service.getPersons().subscribe(
        () => fail('should have failed with 500 error'),
        (error) => {
          expect(error.status).toBe(500);
        }
      );

      const req = httpMock.expectOne(`${apiUrl}?pageNumber=1&pageSize=10`);
      req.flush({ message: 'Internal server error' }, { status: 500, statusText: 'Server Error' });
    });
  });
});
