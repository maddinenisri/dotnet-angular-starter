import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Person, CreatePersonDto, UpdatePersonDto } from '../models/person.model';

@Injectable({
  providedIn: 'root'
})
export class PersonService {
  private http = inject(HttpClient);
  private apiUrl = '/api/persons';

  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  /**
   * Get all persons with pagination
   */
  getPersons(pageNumber: number = 1, pageSize: number = 10): Observable<Person[]> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<Person[]>(this.apiUrl, { params });
  }

  /**
   * Get person by ID
   */
  getPersonById(id: number): Observable<Person> {
    return this.http.get<Person>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create a new person
   */
  createPerson(person: CreatePersonDto): Observable<Person> {
    return this.http.post<Person>(this.apiUrl, person, this.httpOptions);
  }

  /**
   * Update an existing person
   */
  updatePerson(id: number, person: UpdatePersonDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, person, this.httpOptions);
  }

  /**
   * Delete a person
   */
  deletePerson(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Search persons by name
   */
  searchPersons(name: string): Observable<Person[]> {
    const params = new HttpParams().set('name', name);
    return this.http.get<Person[]>(`${this.apiUrl}/search`, { params });
  }
}
