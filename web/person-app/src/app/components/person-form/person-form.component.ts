import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormArray, FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { PersonService } from '../../services/person.service';
import { CreatePersonDto, UpdatePersonDto, Person } from '../../models/person.model';

@Component({
  selector: 'app-person-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './person-form.component.html',
  styleUrls: ['./person-form.component.css']
})
export class PersonFormComponent implements OnInit {
  private fb = inject(FormBuilder);
  private personService = inject(PersonService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  personForm!: FormGroup;
  isEditMode = false;
  personId: number | null = null;
  loading = false;
  errorMessage = '';
  newSkill = '';

  ngOnInit(): void {
    this.initForm();

    // Check if we're in edit mode
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.personId = +params['id'];
        this.loadPerson(this.personId);
      }
    });
  }

  initForm(): void {
    this.personForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      age: ['', [Validators.required, Validators.min(0), Validators.max(150)]],
      dateOfBirth: ['', Validators.required],
      skills: this.fb.array([])
    });
  }

  get skills(): FormArray {
    return this.personForm.get('skills') as FormArray;
  }

  loadPerson(id: number): void {
    this.loading = true;
    this.personService.getPersonById(id).subscribe({
      next: (person) => {
        this.personForm.patchValue({
          name: person.name,
          age: person.age,
          dateOfBirth: this.formatDateForInput(person.dateOfBirth)
        });

        // Add skills
        person.skills.forEach(skill => {
          this.skills.push(this.fb.control(skill));
        });

        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load person data.';
        this.loading = false;
        console.error('Error loading person:', error);
      }
    });
  }

  addSkill(): void {
    if (this.newSkill.trim()) {
      this.skills.push(this.fb.control(this.newSkill.trim()));
      this.newSkill = '';
    }
  }

  removeSkill(index: number): void {
    this.skills.removeAt(index);
  }

  onSubmit(): void {
    if (this.personForm.valid) {
      this.loading = true;
      this.errorMessage = '';

      const formValue = this.personForm.value;
      const personData = {
        name: formValue.name,
        age: formValue.age,
        dateOfBirth: new Date(formValue.dateOfBirth),
        skills: this.skills.value
      };

      if (this.isEditMode && this.personId) {
        // Update existing person
        const updateDto: UpdatePersonDto = personData;
        this.personService.updatePerson(this.personId, updateDto).subscribe({
          next: () => {
            this.loading = false;
            this.router.navigate(['/persons']);
          },
          error: (error) => {
            this.errorMessage = 'Failed to update person. Please try again.';
            this.loading = false;
            console.error('Error updating person:', error);
          }
        });
      } else {
        // Create new person
        const createDto: CreatePersonDto = personData;
        this.personService.createPerson(createDto).subscribe({
          next: () => {
            this.loading = false;
            this.router.navigate(['/persons']);
          },
          error: (error) => {
            this.errorMessage = 'Failed to create person. Please try again.';
            this.loading = false;
            console.error('Error creating person:', error);
          }
        });
      }
    } else {
      // Mark all fields as touched to show validation errors
      Object.keys(this.personForm.controls).forEach(key => {
        this.personForm.get(key)?.markAsTouched();
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/persons']);
  }

  private formatDateForInput(date: Date): string {
    const d = new Date(date);
    const year = d.getUTCFullYear();
    const month = String(d.getUTCMonth() + 1).padStart(2, '0');
    const day = String(d.getUTCDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  // Convenience getters for form controls
  get name() { return this.personForm.get('name'); }
  get age() { return this.personForm.get('age'); }
  get dateOfBirth() { return this.personForm.get('dateOfBirth'); }
}
