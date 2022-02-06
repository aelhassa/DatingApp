import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable, observable } from 'rxjs';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any={};

  constructor(public acountService: AccountService,
       private router:Router,private toaster:ToastrService) { }


  ngOnInit(): void {

  }
  
  login(){
    this.acountService.login(this.model).subscribe(response => {
      console.log(response);
      this.router.navigateByUrl('/members');
    },error =>{
      console.log(error);
      this.toaster.error(error.error);
    })
  }
  logout(){
    this.acountService.logout();
    this.router.navigateByUrl('/');
  }
 
}
