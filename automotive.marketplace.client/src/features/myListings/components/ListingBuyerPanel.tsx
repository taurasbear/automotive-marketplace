import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Link } from "@tanstack/react-router";
import {
  Calendar,
  Clock,
  DollarSign,
  Heart,
  MessageSquare,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useGetOrCreateConversationAsSeller } from "@/features/chat";
import type { ConversationSummary } from "@/features/chat";

import { getListingEngagementsOptions } from "../api/getListingEngagementsOptions";
import type {
  ListingConversationEngagement,
  ListingLikerEngagement,
} from "../types/GetListingEngagementsResponse";

type ListingBuyerPanelProps = {
  listingId: string;
  listingTitle: string;
  listingPrice: number;
  listingMake: string;
  listingMileage: number;
  listingThumbnail: { url: string; altText: string } | null;
  sellerId: string;
  onStartChat: (conversation: ConversationSummary) => void;
};

/** Deterministic color from a UUID string for avatar backgrounds */
function stringToColor(str: string): string {
  let hash = 0;
  for (let i = 0; i < str.length; i++) {
    hash = str.charCodeAt(i) + ((hash << 5) - hash);
  }
  const h = Math.abs(hash) % 360;
  return `hsl(${h}, 60%, 45%)`;
}

function timeAgo(dateStr: string): string {
  const diffMs = Date.now() - new Date(dateStr).getTime();
  const diffHours = Math.floor(diffMs / 3_600_000);
  if (diffHours < 1) return "< 1h";
  if (diffHours < 24) return `${diffHours}h`;
  return `${Math.floor(diffHours / 24)}d`;
}

function InteractionBadge({ type }: { type: string }) {
  const { t } = useTranslation("myListings");
  const config: Record<
    string,
    { icon: React.ReactNode; className: string; label: string }
  > = {
    Offer: {
      icon: <DollarSign className="h-3 w-3" />,
      className: "bg-red-100 text-red-700",
      label: t("buyerPanel.interactionTypes.offer"),
    },
    Meeting: {
      icon: <Calendar className="h-3 w-3" />,
      className: "bg-green-100 text-green-700",
      label: t("buyerPanel.interactionTypes.meeting"),
    },
    Availability: {
      icon: <Clock className="h-3 w-3" />,
      className: "bg-blue-100 text-blue-700",
      label: t("buyerPanel.interactionTypes.availability"),
    },
    Text: {
      icon: <MessageSquare className="h-3 w-3" />,
      className: "bg-gray-100 text-gray-700",
      label: t("buyerPanel.interactionTypes.message"),
    },
  };
  const { icon, className, label } = config[type] ?? config.Text;
  return (
    <span
      className={`flex items-center gap-1 rounded px-2 py-0.5 text-xs ${className}`}
    >
      {icon}
      {label}
    </span>
  );
}

function AvatarInitial({ id, name }: { id: string; name: string }) {
  return (
    <div
      className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-sm font-medium text-white"
      style={{ backgroundColor: stringToColor(id) }}
    >
      {name.slice(0, 1).toUpperCase()}
    </div>
  );
}

export default function ListingBuyerPanel({
  listingId,
  listingTitle,
  listingPrice,
  listingMake,
  listingMileage,
  listingThumbnail,
  sellerId,
  onStartChat,
}: ListingBuyerPanelProps) {
  const { t } = useTranslation("myListings");
  const { mutateAsync: getOrCreate, isPending: isCreating } =
    useGetOrCreateConversationAsSeller();
  const engagementsQuery = useQuery(getListingEngagementsOptions(listingId));

  const conversations = engagementsQuery.data?.data.conversations ?? [];
  const likers = engagementsQuery.data?.data.likers ?? [];

  const handleOpenConversation = (
    engagement: ListingConversationEngagement,
  ) => {
    onStartChat({
      id: engagement.conversationId,
      listingId,
      listingTitle,
      listingThumbnail,
      listingPrice,
      listingMake,
      listingCommercialName: "",
      listingVin: null,
      listingMileage,
      counterpartId: engagement.buyerId,
      counterpartUsername: engagement.buyerUsername,
      lastMessage: null,
      lastMessageAt: engagement.lastInteractionAt,
      unreadCount: 0,
      buyerId: engagement.buyerId,
      sellerId,
      buyerHasEngaged: true,
    });
  };

  const handleOpenLikerChat = async (liker: ListingLikerEngagement) => {
    try {
      const res = await getOrCreate({ listingId, buyerId: liker.userId });
      onStartChat({
        id: res.data.conversationId,
        listingId,
        listingTitle,
        listingThumbnail,
        listingPrice,
        listingMake,
        listingCommercialName: "",
        listingVin: null,
        listingMileage,
        counterpartId: liker.userId,
        counterpartUsername: liker.username,
        lastMessage: null,
        lastMessageAt: new Date().toISOString(),
        unreadCount: 0,
        buyerId: liker.userId,
        sellerId,
        buyerHasEngaged: true,
      });
    } catch {
      // error already handled via mutation meta toast
    }
  };

  if (engagementsQuery.isPending) {
    return (
      <div className="border-border border-t px-4 py-4">
        <div className="space-y-3">
          <Skeleton className="h-10 w-full" />
          <Skeleton className="h-10 w-full" />
        </div>
      </div>
    );
  }

  if (engagementsQuery.isError) {
    return (
      <div className="border-border border-t px-4 py-4">
        <p className="text-destructive text-sm">{t("buyerPanel.loadError")}</p>
      </div>
    );
  }

  const shownConversations = conversations.slice(0, 5);
  const extraConversations = Math.max(0, conversations.length - 5);
  const shownLikers = likers.slice(0, 5);
  const extraLikers = Math.max(0, likers.length - 5);

  return (
    <div className="border-border border-t px-4 py-4">
      <Tabs defaultValue="conversations">
        <TabsList className="mb-3">
          <TabsTrigger value="conversations">
            {t("buyerPanel.conversations")} ({conversations.length})
          </TabsTrigger>
          <TabsTrigger value="likes">
            {t("buyerPanel.likedOnly")} ({likers.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="conversations">
          {shownConversations.length === 0 ? (
            <p className="text-muted-foreground py-2 text-sm">
              {t("buyerPanel.noConversations")}
            </p>
          ) : (
            <div className="space-y-1">
              {shownConversations.map((c) => (
                <div
                  key={c.conversationId}
                  className="flex items-center gap-3 py-2"
                >
                  <AvatarInitial id={c.buyerId} name={c.buyerUsername} />
                  <span className="flex-1 truncate text-sm font-medium">
                    {c.buyerUsername}
                  </span>
                  <InteractionBadge type={c.lastMessageType} />
                  <span className="text-muted-foreground shrink-0 text-xs">
                    {timeAgo(c.lastInteractionAt)}
                  </span>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => handleOpenConversation(c)}
                  >
                    {t("buyerPanel.chat")}
                  </Button>
                </div>
              ))}
              {extraConversations > 0 && (
                <Link
                  to="/inbox"
                  className="text-primary block pt-1 text-sm hover:underline"
                >
                  + {extraConversations} {t("buyerPanel.moreConversations")} →
                </Link>
              )}
            </div>
          )}
        </TabsContent>

        <TabsContent value="likes">
          {shownLikers.length === 0 ? (
            <p className="text-muted-foreground py-2 text-sm">
              {t("buyerPanel.noLikes")}
            </p>
          ) : (
            <div className="space-y-1">
              {shownLikers.map((l) => (
                <div key={l.userId} className="flex items-center gap-3 py-2">
                  <AvatarInitial id={l.userId} name={l.username} />
                  <span className="flex-1 truncate text-sm font-medium">
                    {l.username}
                  </span>
                  <span className="flex items-center gap-1 rounded bg-amber-100 px-2 py-0.5 text-xs text-amber-700">
                    <Heart className="h-3 w-3" />
                    {t("buyerPanel.interactionTypes.liked")}
                  </span>
                  <span className="text-muted-foreground shrink-0 text-xs">
                    {timeAgo(l.likedAt)}
                  </span>
                  <Button
                    size="sm"
                    variant="outline"
                    disabled={isCreating}
                    onClick={() => handleOpenLikerChat(l)}
                  >
                    {t("buyerPanel.chat")}
                  </Button>
                </div>
              ))}
              {extraLikers > 0 && (
                <Link
                  to="/inbox"
                  className="text-primary block pt-1 text-sm hover:underline"
                >
                  + {extraLikers} {t("buyerPanel.moreLikes")} →
                </Link>
              )}
            </div>
          )}
        </TabsContent>
      </Tabs>
    </div>
  );
}
