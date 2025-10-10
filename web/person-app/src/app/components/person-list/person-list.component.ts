import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PersonService } from '../../services/person.service';
import { Person } from '../../models/person.model';

@Component({
  selector: 'app-person-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './person-list.component.html',
  styleUrls: ['./person-list.component.css']
})
export class PersonListComponent implements OnInit {
  private personService = inject(PersonService);

  persons: Person[] = [];
  filteredPersons: Person[] = [];
  searchTerm: string = '';
  pageNumber: number = 1;
  pageSize: number = 10;
  totalCount: number = 0;
  loading: boolean = false;
  errorMessage: string = '';

  ngOnInit(): void {
    this.loadPersons();
  }

  loadPersons(): void {
    this.loading = true;
    this.errorMessage = '';

    this.personService.getPersons(this.pageNumber, this.pageSize).subscribe({
      next: (data) => {
        this.persons = data;
        this.filteredPersons = data;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load persons. Please try again.';
        this.loading = false;
        console.error('Error loading persons:', error);
      }
    });
  }

  searchPersons(): void {
    if (this.searchTerm.trim()) {
      this.loading = true;
      this.personService.searchPersons(this.searchTerm).subscribe({
        next: (data) => {
          this.filteredPersons = data;
          this.loading = false;
        },
        error: (error) => {
          this.errorMessage = 'Search failed. Please try again.';
          this.loading = false;
          console.error('Error searching persons:', error);
        }
      });
    } else {
      this.filteredPersons = this.persons;
    }
  }

  deletePerson(id: number, name: string): void {
    if (confirm(`Are you sure you want to delete ${name}?`)) {
      this.personService.deletePerson(id).subscribe({
        next: () => {
          this.loadPersons();
        },
        error: (error) => {
          this.errorMessage = 'Failed to delete person. Please try again.';
          console.error('Error deleting person:', error);
        }
      });
    }
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.filteredPersons = this.persons;
  }

  previousPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadPersons();
    }
  }

  nextPage(): void {
    this.pageNumber++;
    this.loadPersons();
  }
}
