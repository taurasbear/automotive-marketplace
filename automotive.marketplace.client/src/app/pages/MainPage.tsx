import { ListingSearch } from "@/features/search";
import { Dashboard } from "@/features/dashboard";
import { useAppSelector } from "@/hooks/redux";

const MainPage = () => {
  const userId = useAppSelector((state) => state.auth.userId);

  return (
    <div className="flex flex-1 flex-col">
      {userId && <Dashboard />}
      <ListingSearch className="mt-16 w-full sm:mt-64" />
    </div>
  );
};

export default MainPage;
