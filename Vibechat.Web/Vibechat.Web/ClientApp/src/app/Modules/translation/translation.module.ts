import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ConversationsFormatter} from "../../Formatters/ConversationsFormatter";
import {TranslateComponent} from "../../translate/translate.component";

@NgModule({
  declarations: [
    ConversationsFormatter,
    TranslateComponent
  ],
  entryComponents:[ConversationsFormatter, TranslateComponent],
  exports:[ConversationsFormatter, TranslateComponent],
  imports: [
    CommonModule
  ]
})
export class TranslationModule { }
