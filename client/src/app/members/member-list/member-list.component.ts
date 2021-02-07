import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
members:Member[];
//members$:Observable<Member[]>;
pagination:Pagination;
// pageNumber=1;
// pageSize=5;
userparams:UserParams;
user:User;
genderList=[{value:'male',display:'Males'},{value:'female',display:'Females'}];
constructor(private memberService:MembersService) {
  // constructor(private memberService:MembersService,private accountservice:AccountService) {
    // this.accountservice.currentUser$.pipe(take(1)).subscribe(user=>{
    //   this.user=user;
    //   this.userparams=new UserParams(user);
    // })
    this.userparams=this.memberService.getUserParams();
   }

  ngOnInit(): void {
    this.loadMembers();
    //this.members$=this.memberService.getMembers();
  }

  loadMembers(){
    this.memberService.setUserParams(this.userparams);
    // this.memberService.getMembers(this.pageNumber,this.pageSize).subscribe(response=>{
      this.memberService.getMembers(this.userparams).subscribe(response=>{
      this.members=response.result;
      this.pagination=response.pagination;
    })
  }

  resetFilters(){
    // this.userparams=new UserParams(this.user);
    this.userparams=this.memberService.resetUserParams();
    this.loadMembers();
  }
  pageChanged(event:any){
    // this.pageNumber=event.page;
    this.userparams.pageNumber=event.page;
    this.memberService.setUserParams(this.userparams);
    this.loadMembers();
  }

}
