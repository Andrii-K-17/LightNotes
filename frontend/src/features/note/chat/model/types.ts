export interface Message {
  id: string;
  noteId: string;
  senderId: string;
  senderName: string;
  text: string;
  timestamp: string;
}

export interface SendMessageRequest {
  text: string;
}
