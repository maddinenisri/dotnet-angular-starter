export interface Person {
  id: number;
  name: string;
  age: number;
  dateOfBirth: Date;
  skills: string[];
  createdAt?: Date;
  updatedAt?: Date;
}

export interface CreatePersonDto {
  name: string;
  age: number;
  dateOfBirth: Date;
  skills: string[];
}

export interface UpdatePersonDto {
  name: string;
  age: number;
  dateOfBirth: Date;
  skills: string[];
}
