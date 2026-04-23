import { useDateLocale } from "@/lib/i18n/dateLocale";
import { useSuspenseQuery } from "@tanstack/react-query";
import { formatDistanceToNow } from "date-fns";
import { useEffect, useRef } from "react";
import { useTranslation } from "react-i18next";
import { getConversationsOptions } from "../api/getConversationsOptions";
import type { ConversationSummary } from "../types/ConversationSummary";

type ConversationListProps = {
  selectedId: string | null;
  onSelect: (conversation: ConversationSummary) => void;
  initialConversationId?: string;
  onInitialLoad?: (conversation: ConversationSummary | null) => void;
};

const ConversationList = ({
  selectedId,
  onSelect,
  initialConversationId,
  onInitialLoad,
}: ConversationListProps) => {
  const { data: conversationsQuery } = useSuspenseQuery(
    getConversationsOptions(),
  );
  const conversations = conversationsQuery.data;
  const { t } = useTranslation("chat");
  const locale = useDateLocale();
  const didAutoSelect = useRef(false);

  useEffect(() => {
    if (didAutoSelect.current || !initialConversationId || !onInitialLoad)
      return;
    didAutoSelect.current = true;
    const match = conversations.find((c) => c.id === initialConversationId);
    onInitialLoad(match ?? null);
  }, [conversations, initialConversationId, onInitialLoad]);

  if (conversations.length === 0) {
    return (
      <div className="text-muted-foreground flex h-full items-center justify-center p-4 text-sm">
        {t("conversationList.noConversationsYet")}
      </div>
    );
  }

  return (
    <ul className="divide-border divide-y overflow-y-auto">
      {conversations.map((c) => (
        <li
          key={c.id}
          onClick={() => onSelect(c)}
          className={`hover:bg-muted/50 cursor-pointer px-4 py-3 transition-colors ${
            selectedId === c.id ? "bg-muted" : ""
          }`}
        >
          <div className="flex items-start gap-3">
            {c.listingThumbnail ? (
              <img
                src={c.listingThumbnail.url}
                alt={c.listingThumbnail.altText || c.listingTitle}
                className="mt-0.5 h-10 w-14 shrink-0 rounded object-cover"
              />
            ) : (
              <div className="bg-muted mt-0.5 h-10 w-14 shrink-0 rounded" />
            )}
            <div className="min-w-0 flex-1">
              <div className="flex items-center justify-between gap-1">
                <span className="truncate text-sm font-semibold">
                  {c.listingTitle}
                </span>
                {c.unreadCount > 0 && (
                  <span className="bg-primary text-primary-foreground shrink-0 rounded-full px-1.5 py-0.5 text-[10px] font-bold">
                    {c.unreadCount}
                  </span>
                )}
              </div>
              <p className="text-muted-foreground truncate text-xs">
                {c.counterpartUsername}
              </p>
              {c.lastMessage && (
                <p className="text-muted-foreground truncate text-xs">
                  {c.lastMessage}
                </p>
              )}
            </div>
          </div>
          <p className="text-muted-foreground mt-1 text-right text-[10px]">
            {formatDistanceToNow(new Date(c.lastMessageAt), {
              addSuffix: true,
              locale,
            })}
          </p>
        </li>
      ))}
    </ul>
  );
};

export default ConversationList;
