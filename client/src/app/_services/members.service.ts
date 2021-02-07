import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { of } from 'rxjs';
import { Observable } from 'rxjs';
import { map, take } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { PaginatedResult } from '../_models/pagination';
import { User } from '../_models/user';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
//implemented jwt interceptor for authorization
// const httpOptions={
//   headers:new HttpHeaders({
//     Authorization:'Bearer '+JSON.parse(localStorage.getItem('user'))?.token
//   })
// }
@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl=environment.apiUrl;
  members:Member[]=[];
  // paginatedResult:PaginatedResult<Member[]>=new PaginatedResult<Member[]>();
memberCache=new Map();
user:User;
userparams:UserParams;
  constructor(private http:HttpClient,private accountService:AccountService) { 
    this.accountService.currentUser$.pipe(take(1)).subscribe(user=>{
      this.user=user;
      this.userparams=new UserParams(user);
    })
  }
  getUserParams(){
    return this.userparams;
  }
  setUserParams(params:UserParams){
    this.userparams=params;
  }
  resetUserParams(){
    this.userparams=new UserParams(this.user);
    return this.userparams;
  }
  // getMembers():Observable<Member[]>{
  // getMembers(){
  //   // return this.http.get<Member[]>(this.baseUrl+'users',httpOptions);
  //  // return this.http.get<Member[]>(this.baseUrl+'users');
  //   if(this.members.length>0) return of(this.members);
  //    return this.http.get<Member[]>(this.baseUrl+'users').pipe(
  //      map(members=>{
  //       this.members=members;
  //       return members;
  //      })
  //    )
  // }



  // getMembers(page?:number,itemsPerPage?:number){
  //   let params=new HttpParams();
  //   if(page!==null&&itemsPerPage!==null){
  //     params=params.append('pageNumber',page.toString());
  //     params=params.append('pageSize',itemsPerPage.toString());
  //   }
  //    return this.http.get<Member[]>(this.baseUrl+'users',{observe:'response',params}).pipe(
  //      map(response=>{
  //        this.paginatedResult.result=response.body;
  //        if(response.headers.get('Pagination')!==null){
  //          this.paginatedResult.pagination=JSON.parse(response.headers.get('Pagination'));
  //        }
  //        return this.paginatedResult;
  //      })
  //    )
  // }

  getMembers(userparams:UserParams){
    //console.log(Object.values(userparams).join('-'));
    var response=this.memberCache.get(Object.values(userparams).join('-'));
    if(response){
      return of(response);
    }
   let params=this.getPaginationHeaders(userparams.pageNumber,userparams.pageSize);
   
   params=params.append('minAge',userparams.minAge.toString());
   params=params.append('maxAge',userparams.maxAge.toString());
   params=params.append('gender',userparams.gender);
   params=params.append('orderBy',userparams.orderBy);

     return this.getPaginatedResult<Member[]>(this.baseUrl+'users',params)
     .pipe(map(response=>{
       this.memberCache.set(Object.values(userparams).join('-'),response);
       return response;
     }))
  }

  private getPaginatedResult<T>(url,params) {
    const paginatedResult:PaginatedResult<T>=new PaginatedResult<T>();
    return this.http.get<T>(url, { observe: 'response', params }).pipe(
      map(response => {
        paginatedResult.result = response.body;
        if (response.headers.get('Pagination') !== null) {
          paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
        }
        return paginatedResult;
      })
    );
  }

  private getPaginationHeaders(pageNumber:number,pageSize:number){
    let params=new HttpParams();
    
      params=params.append('pageNumber',pageNumber.toString());
      params=params.append('pageSize',pageSize.toString());
    return params;
  }
  getMember(username:string){
    // return this.http.get<Member>(this.baseUrl+'user/'+username,httpOptions);
   // return this.http.get<Member>(this.baseUrl+'users/'+username);
   
   //const member=this.members.find(x=>x.username===username);
   //if(member!==undefined)return of(member);
   //console.log(this.memberCache);

  //  const member=[...this.memberCache.values()];
   //console.log(member);

  //  const member=[...this.memberCache.values()]
  //  .reduce((arr,elem)=>arr.concat(elem.result),[]);
  //  console.log(member);

  const member=[...this.memberCache.values()]
   .reduce((arr,elem)=>arr.concat(elem.result),[])
   .find((member:Member)=>member.username===username);
   
   if(member){
     return of(member);
   }
   return this.http.get<Member>(this.baseUrl+'users/'+username);

  }
  updateMember(member:Member){
    //return this.http.put(this.baseUrl+'users',member);
    return this.http.put(this.baseUrl+'users',member).pipe(
      map(()=>{
        const index=this.members.indexOf(member);
        this.members[index]=member;
      })
    )
  }

  setMainPhoto(photoId:number){
    return this.http.put(this.baseUrl+'users/set-main-photo/'+photoId,{});
  }

  deletePhoto(photoId:number){
    return this.http.delete(this.baseUrl+'users/delete-photo/'+photoId);
  }
}
