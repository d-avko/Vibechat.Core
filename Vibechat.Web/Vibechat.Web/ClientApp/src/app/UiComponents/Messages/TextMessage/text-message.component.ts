import {Component, Input, OnChanges, OnInit, SimpleChanges} from '@angular/core';
import {DomSanitizer, SafeHtml} from "@angular/platform-browser";
import * as normalizeUrl from "normalize-url";

@Component({
  selector: 'text-message',
  templateUrl: './text-message.component.html',
  styleUrls: ['./text-message.component.css']
})
export class TextMessageComponent implements OnChanges, OnInit {

  @Input() content: string;

  public Content: SafeHtml;

  private static LinksRegex = new RegExp(`[-a-zA-Z0-9@:%._+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_+.~#?&//=]*)`, "g");

  private static RawLinkRegex = new RegExp(`@@link`);

  private static LinkHtml = '<a style="color: #5b89a0;text-decoration: none;" href="@@link">@@link</a>';

  constructor(private sanitizer: DomSanitizer) { }

  ngOnInit() {
    this.ParseLinks();
  }

  private LinksParsing = false;

  private ParseLinks(){
    if(!this.content && !this.LinksParsing){
      return;
    }

    try {
      this.LinksParsing = true;

      let linksRaw = this.content.replace(TextMessageComponent.LinksRegex, this.replaceTextLinkWithLinkTag.bind(this));
      let match : RegExpExecArray;
      let index = 0;

      do{
        match = TextMessageComponent.RawLinkRegex.exec(linksRaw);

        if(match){
          linksRaw = this.splice(linksRaw, match.index, match[0].length, this.normalizeUrl(this.LinksFound[index]));

          match = TextMessageComponent.RawLinkRegex.exec(linksRaw);

          //execute twice, for href and innerText
          //this is for innerText
          if(match) {
            linksRaw = this.splice(linksRaw, match.index, match[0].length, this.LinksFound[index]);
          }
        }

        ++index;
      }while(match);

      this.Content = this.sanitizer.bypassSecurityTrustHtml(linksRaw);
    }
    finally {
      this.LinksParsing = false;
      this.LinksFound = new Array<string>();
    }
  }

  private splice(str : string, index : number, count: number, add: string) {
    // We cannot pass negative indexes directly to the 2nd slicing operation.
    if (index < 0) {
      index = str.length + index;
      if (index < 0) {
        index = 0;
      }
    }

    return str.slice(0, index) + (add || "") + str.slice(index + count);
  }

  private LinksFound = new Array<string>();

  private normalizeUrl(url: string) : string{
    return normalizeUrl(url);
  }

  private replaceTextLinkWithLinkTag(substring: string, ...args: any) : string{
    if(!substring){
      return "";
    }

    this.LinksFound.push(substring);
    return TextMessageComponent.LinkHtml;
  }


  ngOnChanges(changes: SimpleChanges): void {
    if(changes.content){
      this.ParseLinks();
    }
  }
}
