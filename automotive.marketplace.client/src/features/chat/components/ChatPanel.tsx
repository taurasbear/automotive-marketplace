import { Button } from "@/components/ui/button";
import { Spinner } from "@/components/ui/shadcn-io/spinner";
import { Suspense, useEffect } from "react";
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
        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-semibold">
            {conversation.counterpartUsername}
          </p>
          {conversation.listingTitle && (
            <p className="text-muted-foreground truncate text-xs">
              {conversation.listingTitle}
            </p>
          )}
        </div>
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
        <Suspense fallback={
          <div className="flex h-full items-center justify-center" role="status" aria-label="Loading">
            <Spinner />
          </div>
        }>
          <MessageThread conversation={conversation} showListingCard={false} />
        </Suspense>
      </div>
    </div>
  );
};

export default ChatPanel;
