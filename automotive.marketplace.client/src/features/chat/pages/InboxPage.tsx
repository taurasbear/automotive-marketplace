import { useState } from "react";
import ConversationList from "../components/ConversationList";
import MessageThread from "../components/MessageThread";
import type { ConversationSummary } from "../types/ConversationSummary";

const InboxPage = () => {
  const [selected, setSelected] = useState<ConversationSummary | null>(null);

  return (
    <div className="flex h-[calc(100vh-64px)] overflow-hidden">
      <aside className="border-border w-72 shrink-0 overflow-hidden border-r lg:w-80">
        <div className="border-border border-b px-4 py-3">
          <h1 className="text-lg font-semibold">Messages</h1>
        </div>
        <ConversationList
          selectedId={selected?.id ?? null}
          onSelect={setSelected}
        />
      </aside>

      <main className="flex min-w-0 flex-1 flex-col">
        {selected ? (
          <MessageThread conversation={selected} />
        ) : (
          <div className="text-muted-foreground flex h-full items-center justify-center text-sm">
            Select a conversation to start messaging.
          </div>
        )}
      </main>
    </div>
  );
};

export default InboxPage;
