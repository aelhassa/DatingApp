import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  @Input() usersFromHomeComponent : any;
  @Output() cancelRegister = new EventEmitter();
  registerForm:FormGroup;
  maxDate:Date;
  validationErrors:string[]=[];
  
  constructor(private accountService:AccountService,
    private toaster:ToastrService,private router :Router,
    private fb:FormBuilder) { }

  ngOnInit(): void {
   this.intitializeForm();
   this.maxDate=new Date();
   this.maxDate.setFullYear(this.maxDate.getFullYear()-18);
  }
  intitializeForm(){
    this.registerForm=this.fb.group({
      
      gender:['male'],
      username:['',Validators.required],
      KnownAs:['',Validators.required],
      dateOfBirth:['',Validators.required],
      city:['',Validators.required],
      country:['',Validators.required],
      password:['',[Validators.required,Validators.minLength(4)
        ,Validators.maxLength(8)]],
      confirmPassword:['',
      [Validators.required,this.matchValue('password')]]

    });
    this.registerForm.controls.password.valueChanges.subscribe(()=>{
      this.registerForm.controls.confirmPassword.updateValueAndValidity();
    });
  }
  matchValue(matchTo:string):ValidatorFn{
   return (control:AbstractControl)=>{
     return control?.value===control?.parent?.controls[matchTo].value ?null 
     :{isMatching:true}
   }
  }
  
  register(){
    this.accountService.register(this.registerForm.value).subscribe(response=>{
      console.log(response);
      this.router.navigateByUrl('/members');
      this.cancel();
    },error =>{
      console.log(error);
      this.validationErrors=error;
    })
  }
  cancel(){
    this.cancelRegister.emit(false);
  }
}
