import { ListingSearch } from "@/features/search";

const MainPage = () => {
  return (
    <div className="bg-secondary mx-auto mt-64 rounded-sm p-4">
      <label className="block pb-5 text-2xl font-semibold">Look up</label>
      <ListingSearch className="w-full" />
    </div>
  );
};

export default MainPage;
