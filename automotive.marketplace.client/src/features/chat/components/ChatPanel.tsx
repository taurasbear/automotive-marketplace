import { Button } from "@/components/ui/button";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import type { ConversationSummary } from "../types/ConversationSummary";
import MessageThread from "./MessageThread";

type ChatPanelProps = {
  conversation: ConversationSummary;
  onClose: () => void;
};

const ChatPanel = ({ conversation, onClose }: ChatPanelProps) => {
  const { t } = useTranslation("chat");
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
        <Button
          variant="ghost"
          size="sm"
          onClick={onClose}
          aria-label={t("chatPanel.closeChat")}
          className="text-muted-foreground hover:text-foreground h-auto p-1 leading-none"
        >
          ✕
        </Button>
      </div>
      <div className="flex-1 overflow-hidden">
        <MessageThread conversation={conversation} showListingCard={false} />
      </div>
    </div>
  );
};

export default ChatPanel;
