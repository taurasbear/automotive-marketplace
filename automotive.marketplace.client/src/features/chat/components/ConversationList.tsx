import { useSuspenseQuery } from "@tanstack/react-query";
import { formatDistanceToNow } from "date-fns";
import { getConversationsOptions } from "../api/getConversationsOptions";
import type { ConversationSummary } from "../types/ConversationSummary";

type ConversationListProps = {
  selectedId: string | null;
  onSelect: (conversation: ConversationSummary) => void;
};

const ConversationList = ({ selectedId, onSelect }: ConversationListProps) => {
  const { data: conversationsQuery } = useSuspenseQuery(
    getConversationsOptions(),
  );
  const conversations = conversationsQuery.data;

  if (conversations.length === 0) {
    return (
      <div className="text-muted-foreground flex h-full items-center justify-center p-4 text-sm">
        No conversations yet.
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
            {c.listingThumbnailUrl ? (
              <img
                src={c.listingThumbnailUrl}
                alt={c.listingTitle}
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
            })}
          </p>
        </li>
      ))}
    </ul>
  );
};

export default ConversationList;
