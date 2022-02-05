import { Component, OnInit } from '@angular/core';
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

  constructor(public acountService: AccountService) { }


  ngOnInit(): void {

  }
  
  login(){
    this.acountService.login(this.model).subscribe(response => {
      console.log(response);
    },error =>{
      console.log(error);
    })
  }
  logout(){
    this.acountService.logout();
  }
 
}
