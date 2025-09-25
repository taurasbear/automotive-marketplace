import { CarListTable, CreateCarDialog } from "@/features/carList";

const Cars = () => {
  return (
    <div className="grid items-center justify-center space-y-5 pt-10">
      <div className="flex w-full justify-end">
        <CreateCarDialog />
      </div>
      <CarListTable className="max-w-3xl" />
    </div>
  );
};

export default Cars;
