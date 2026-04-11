import { useAppSelector } from "@/hooks/redux";
import { selectUnreadCount } from "../state/chatSlice";

const UnreadBadge = () => {
  const count = useAppSelector(selectUnreadCount);
  if (count === 0) return null;
  return (
    <span className="bg-destructive text-destructive-foreground absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full text-[10px] font-bold">
      {count > 99 ? "99+" : count}
    </span>
  );
};

export default UnreadBadge;
