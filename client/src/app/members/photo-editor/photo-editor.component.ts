import { Component, inject, input, OnInit, output } from '@angular/core';
import { Member } from '../../_models/member';
import { DecimalPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FileUploader, FileUploadModule } from 'ng2-file-upload';
import { AccountService } from '../../_services/account.service';
import { environment } from '../../../environments/environment';
import { Photo } from '../../_models/photo';
import { MembersService } from '../../_services/members.service';

@Component({
  selector: 'app-photo-editor',
  standalone: true,
  imports: [NgIf, NgFor, NgStyle, NgClass, FileUploadModule, DecimalPipe],
  templateUrl: './photo-editor.component.html',
  styleUrl: './photo-editor.component.css'
})
export class PhotoEditorComponent implements OnInit {
  private memberService = inject(MembersService);
  private accountService = inject(AccountService);
  member = input.required<Member>();
  uploader?: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  memberChange = output<Member>();

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any) {
    this.hasBaseDropZoneOver = true;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.accountService.CurrentUser()?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      const photo = JSON.parse(response);
      const updatedMember = { ...this.member() }
      updatedMember.photos.push(photo);
      this.memberChange.emit(updatedMember);
      if(photo.isMain){
        const user = this.accountService.CurrentUser();
        if (user) {
          user.photoUrl = photo.url,
            this.accountService.setCurrentUser(user)
        }
        updatedMember.photoUrl = photo.url,
          updatedMember.photos.forEach(p => {
            if (p.isMain) p.isMain = false;
            if (p.id == photo.id) p.isMain = true;
          });
        this.memberChange.emit(updatedMember);
      }
    }
  }

  deletePhoto(photoId: number) {
   this.memberService.deletePhoto(photoId).subscribe({
    next:_=>{
      const updatedMember ={...this.member()};
      updatedMember.photos = updatedMember.photos.filter(x=>x.id!==photoId);
      this.memberChange.emit(updatedMember);
    }
   })
  }
  
  setMainPhoto(Photo: Photo) {
    this.memberService.setMainPhoto(Photo.id).subscribe({
      next: _ => {
        const user = this.accountService.CurrentUser();
        if (user) {
          user.photoUrl = Photo.url,
            this.accountService.setCurrentUser(user)
        }
        const updatedMember = { ...this.member() }
        updatedMember.photoUrl = Photo.url,
          updatedMember.photos.forEach(p => {
            if (p.isMain) p.isMain = false;
            if (p.id == Photo.id) p.isMain = true;
          });
        this.memberChange.emit(updatedMember);
      }
    })
  }

 
}
