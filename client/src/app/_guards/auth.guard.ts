import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
 
  constructor(private accoutService :AccountService,private toaster:ToastrService){}
  canActivate(): Observable<boolean>{
    return this.accoutService.currentUser$.pipe(map(user=>{
      if(user) return true;
      this.toaster.error('you shall not pass!');
    }))
  }
  
}
