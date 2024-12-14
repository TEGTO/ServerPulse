// import { FormControl, FormGroup } from "@angular/forms";
// import { confirmPasswordValidator } from "./password-confirmation-validator";

// describe('confirmPasswordValidator', () => {
//     let formGroup: FormGroup;

//     beforeEach(() => {
//         formGroup = new FormGroup({
//             password: new FormControl(''),
//             confirmPassword: new FormControl('', confirmPasswordValidator),
//         });
//     });

//     it('should return null if passwords match', () => {
//         formGroup.controls['password'].setValue('password123');
//         formGroup.controls['confirmPassword'].setValue('password123');

//         expect(formGroup.controls['confirmPassword'].errors).toBeNull();
//     });

//     it('should return { passwordNoMatch: true } if passwords do not match', () => {
//         formGroup.controls['password'].setValue('password123');
//         formGroup.controls['confirmPassword'].setValue('password456');

//         expect(formGroup.controls['confirmPassword'].errors).toEqual({ passwordNoMatch: true });
//     });

//     it('should return null if form group is not provided', () => {
//         const control = new FormControl('password123');
//         expect(confirmPasswordValidator(control)).toBeNull();
//     });
// });