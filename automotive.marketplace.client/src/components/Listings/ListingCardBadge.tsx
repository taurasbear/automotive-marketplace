interface ListingCardBadgeProps {
  Icon: React.ReactNode;
  title: string;
  stat: string;
}

const ListingCardBadge: React.FC<ListingCardBadgeProps> = ({
  Icon,
  title,
  stat,
}) => {
  return (
    <div className="border-primary-border flex w-full max-w-40 flex-row items-center justify-center gap-3 rounded-md border p-1">
      {Icon}
      <div className="flex w-28 flex-col">
        <p className="font-sans text-sm">{title}</p>
        <p className="truncate font-sans text-sm font-semibold">{stat}</p>
      </div>
    </div>
  );
};

export default ListingCardBadge;
