import { HttpClient, HttpHeaders, HttpParams, HttpResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { AccountService } from './account.service';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/UserParams';
import { of } from 'rxjs';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';


@Injectable({
  providedIn: 'root'
})
export class MembersService {
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  baseUrl = environment.apiUrl;
  paginatedResult = signal<PaginatedResult<Member[]> | null>(null);
  memberCache = new Map();


  getMembers(userParams: UserParams) {
    const response = this.memberCache.get(Object.values(userParams).join('-'));
    if (response) return setPaginatedResponse(response,this.paginatedResult);
    
    
    let params = setPaginationHeaders(userParams.pageNumber, userParams.pageSize)

    params = params.append('minAge', userParams.minAge);
    params = params.append('maxAge', userParams.maxAge);
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);

    return this.http.get<Member[]>(this.baseUrl + 'users', { observe: 'response', params }).subscribe({
      next: response => {
        setPaginatedResponse(response,this.paginatedResult);
        this.memberCache.set(Object.values(userParams).join('-'),response);
      }
    })
  }

  

  getMember(username: string) {
    const member:Member = [...this.memberCache.values()]
    .reduce((arr,elem)=>arr.concat(elem.body),[])
    .find((m:Member)=>m.username == username);

    if(member) return of(member);
    
    return this.http.get<Member>(this.baseUrl + 'users/' + username, this.getHttpOptions());
  }

  updateMember(member: Member) {
    return this.http.put<Member>(this.baseUrl + 'users', member).pipe(
    )
  }

  getHttpOptions() {
    return {
      headers: new HttpHeaders({
        Authorization: `Bearer ${this.accountService.CurrentUser()?.token}`
      })
    }
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {})
  }

  deletePhoto(PhotoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + PhotoId);
  }
}
