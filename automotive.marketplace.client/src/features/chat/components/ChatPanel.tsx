import { useEffect } from "react";
import type { ConversationSummary } from "../types/ConversationSummary";
import MessageThread from "./MessageThread";

type ChatPanelProps = {
  conversation: ConversationSummary;
  onClose: () => void;
};

const ChatPanel = ({ conversation, onClose }: ChatPanelProps) => {
  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => e.key === "Escape" && onClose();
    window.addEventListener("keydown", handleKey);
    return () => window.removeEventListener("keydown", handleKey);
  }, [onClose]);

  return (
    <div className="border-border bg-card fixed inset-y-0 right-0 z-50 flex w-80 flex-col border-l shadow-xl lg:w-96">
      <div className="border-border flex items-center gap-3 border-b px-4 py-3">
        <p className="flex-1 text-sm font-semibold">
          {conversation.counterpartUsername}
        </p>
        <button
          onClick={onClose}
          className="text-muted-foreground hover:text-foreground text-lg leading-none"
          aria-label="Close chat"
        >
          ✕
        </button>
      </div>
      <div className="flex-1 overflow-hidden">
        <MessageThread conversation={conversation} showListingCard={false} />
      </div>
    </div>
  );
};

export default ChatPanel;
