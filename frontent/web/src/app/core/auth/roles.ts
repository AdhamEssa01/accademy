export const Roles = {
  Admin: 'Admin',
  Instructor: 'Instructor',
  Parent: 'Parent',
  Student: 'Student',
} as const;

export type Role = (typeof Roles)[keyof typeof Roles];
