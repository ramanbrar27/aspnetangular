// import { HttpClient } from '@angular/common/http';
import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import{User} from './_models/user';
import { AccountService } from './_services/account.service';
import { PresenceService } from './_services/presence.service';
@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'The Dating App';
  users:any;
  // constructor(private http:HttpClient,private accountService:AccountService) {
    constructor(private accountService:AccountService,
      private presence:PresenceService) {
    
  }
  ngOnInit(){
    //this.getUsers();
    this.setCurrentUser();
  }
  
  setCurrentUser(){
    const user:User=JSON.parse(localStorage.getItem('user'));
    if(user){
      this.accountService.setCurrentUser(user);
      this.presence.createHubConnection(user);
    }
   // this.accountService.setCurrentUser(user);
  }

  // getUsers(){
  //   this.http.get('http://localhost:5001/api/users').subscribe(response=>{
  //   this.users=response;
  //   },error=>{
  //     console.log(error);
  //   })
  // }
}
