-- ==============================================================================
-- MUE2MUEDEE (มือสองมือดี)
-- Complete Seed Script: สร้างข้อมูลสินค้าในคลัง + รูปภาพแกลเลอรี 3 ชุดพร้อมใช้งานทันที
-- Database: PostgreSQL (Neon)
-- ==============================================================================

DO $$
DECLARE
    v_store_id UUID;
    
    -- Brand IDs
    v_brand_sony UUID;
    v_brand_fuji UUID;
    v_brand_canon UUID;

    -- Category IDs
    v_cat_camera UUID;
    v_cat_lens UUID;

    -- Grade IDs
    v_grade_aplus UUID;
    v_grade_a UUID;

    -- Product IDs
    v_prod_sony UUID;
    v_prod_fuji UUID;
    v_prod_canon UUID;

    -- Inventory Item IDs
    v_item_sony UUID := gen_random_uuid();
    v_item_fuji UUID := gen_random_uuid();
    v_item_canon UUID := gen_random_uuid();
BEGIN
    -- --------------------------------------------------------------------------
    -- 1. ค้นหาร้านค้า (หรือสร้างร้านค่าเริ่มต้นถ้ายังไม่มี)
    -- --------------------------------------------------------------------------
    SELECT id INTO v_store_id FROM stores WHERE is_active = true LIMIT 1;

    IF v_store_id IS NULL THEN
        v_store_id := gen_random_uuid();
        INSERT INTO stores (id, name, code, phone, email, address, primary_color, is_active, created_at, created_by)
        VALUES (v_store_id, 'ABC Camera — ร้านกล้องและเลนส์มือสอง', 'ABCCAM', '081-234-5678', 'contact@abccamera.com',
                '123 ถนนสุขุมวิท แขวงคลองเตย เขตคลองเตย กรุงเทพฯ 10110', '#4f46e5', true, NOW(), 'system');
        RAISE NOTICE 'Created default store ABCCAM (ID: %)', v_store_id;
    END IF;

    -- --------------------------------------------------------------------------
    -- 2. สร้างแบรนด์ (Brands) ถ้ายังไม่มี
    -- --------------------------------------------------------------------------
    -- Sony
    SELECT id INTO v_brand_sony FROM brands WHERE store_id = v_store_id AND name = 'Sony';
    IF v_brand_sony IS NULL THEN
        v_brand_sony := gen_random_uuid();
        INSERT INTO brands (id, store_id, name, description, is_active, is_deleted, created_at, created_by)
        VALUES (v_brand_sony, v_store_id, 'Sony', 'กล้องและเลนส์ระบบ E-mount', true, false, NOW(), 'seed');
    END IF;

    -- Fujifilm
    SELECT id INTO v_brand_fuji FROM brands WHERE store_id = v_store_id AND name = 'Fujifilm';
    IF v_brand_fuji IS NULL THEN
        v_brand_fuji := gen_random_uuid();
        INSERT INTO brands (id, store_id, name, description, is_active, is_deleted, created_at, created_by)
        VALUES (v_brand_fuji, v_store_id, 'Fujifilm', 'กล้อง X-Series สไตล์คลาสสิกเรโทร', true, false, NOW(), 'seed');
    END IF;

    -- Canon
    SELECT id INTO v_brand_canon FROM brands WHERE store_id = v_store_id AND name = 'Canon';
    IF v_brand_canon IS NULL THEN
        v_brand_canon := gen_random_uuid();
        INSERT INTO brands (id, store_id, name, description, is_active, is_deleted, created_at, created_by)
        VALUES (v_brand_canon, v_store_id, 'Canon', 'กล้องและเลนส์ระบบ RF/EF mount', true, false, NOW(), 'seed');
    END IF;

    -- --------------------------------------------------------------------------
    -- 3. สร้างหมวดหมู่ (Categories) ถ้ายังไม่มี
    -- Type 1 = Camera, Type 2 = Lens
    -- --------------------------------------------------------------------------
    SELECT id INTO v_cat_camera FROM categories WHERE store_id = v_store_id AND name = 'กล้อง Mirrorless';
    IF v_cat_camera IS NULL THEN
        v_cat_camera := gen_random_uuid();
        INSERT INTO categories (id, store_id, name, type, description, is_active, is_deleted, created_at, created_by)
        VALUES (v_cat_camera, v_store_id, 'กล้อง Mirrorless', 1, 'กล้องดิจิทัลเปลี่ยนเลนส์ได้', true, false, NOW(), 'seed');
    END IF;

    SELECT id INTO v_cat_lens FROM categories WHERE store_id = v_store_id AND name = 'เลนส์ฟิกซ์ (Prime Lens)';
    IF v_cat_lens IS NULL THEN
        v_cat_lens := gen_random_uuid();
        INSERT INTO categories (id, store_id, name, type, description, is_active, is_deleted, created_at, created_by)
        VALUES (v_cat_lens, v_store_id, 'เลนส์ฟิกซ์ (Prime Lens)', 2, 'เลนส์ระยะคงที่ คุณภาพสูง', true, false, NOW(), 'seed');
    END IF;

    -- --------------------------------------------------------------------------
    -- 4. สร้างเกรดสภาพสินค้า (Condition Grades) ถ้ายังไม่มี
    -- --------------------------------------------------------------------------
    SELECT id INTO v_grade_aplus FROM condition_grades WHERE store_id = v_store_id AND grade_code = 'A+';
    IF v_grade_aplus IS NULL THEN
        v_grade_aplus := gen_random_uuid();
        INSERT INTO condition_grades (id, store_id, grade_code, grade_name, description, sort_order, is_active, is_deleted, created_at, created_by)
        VALUES (v_grade_aplus, v_store_id, 'A+', 'สภาพนางฟ้า (Mint 98-99%)', 'สภาพเสมือนของใหม่ ไร้รอยขนแมว อุปกรณ์ครบกล่อง', 1, true, false, NOW(), 'seed');
    END IF;

    SELECT id INTO v_grade_a FROM condition_grades WHERE store_id = v_store_id AND grade_code = 'A';
    IF v_grade_a IS NULL THEN
        v_grade_a := gen_random_uuid();
        INSERT INTO condition_grades (id, store_id, grade_code, grade_name, description, sort_order, is_active, is_deleted, created_at, created_by)
        VALUES (v_grade_a, v_store_id, 'A', 'สภาพสวยงาม (Near Mint 95%)', 'สภาพสวยมาก มีริ้วรอยขนแมวบางๆ ตามการใช้งานเล็กน้อย', 2, true, false, NOW(), 'seed');
    END IF;

    -- --------------------------------------------------------------------------
    -- 5. สร้างสินค้ารุ่นมาสเตอร์ (Product Masters) ถ้ายังไม่มี
    -- --------------------------------------------------------------------------
    -- Sony A7 IV
    SELECT id INTO v_prod_sony FROM products WHERE store_id = v_store_id AND product_code = 'SONY-A7M4';
    IF v_prod_sony IS NULL THEN
        v_prod_sony := gen_random_uuid();
        INSERT INTO products (id, store_id, brand_id, category_id, model_name, product_code, description, default_selling_price, image_url, is_active, is_deleted, created_at, created_by)
        VALUES (v_prod_sony, v_store_id, v_brand_sony, v_cat_camera, 'Alpha 7 IV (Body)', 'SONY-A7M4', 'กล้องฟูลเฟรม 33MP ระบบโฟกัส AI Real-time Eye AF รองรับวิดีโอ 4K 60p 10-bit', 64900.00,
                'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=1200&q=80', true, false, NOW(), 'seed');
    END IF;

    -- Fujifilm X-T5
    SELECT id INTO v_prod_fuji FROM products WHERE store_id = v_store_id AND product_code = 'FUJI-XT5';
    IF v_prod_fuji IS NULL THEN
        v_prod_fuji := gen_random_uuid();
        INSERT INTO products (id, store_id, brand_id, category_id, model_name, product_code, description, default_selling_price, image_url, is_active, is_deleted, created_at, created_by)
        VALUES (v_prod_fuji, v_store_id, v_brand_fuji, v_cat_camera, 'X-T5 Body (Black)', 'FUJI-XT5', 'กล้องเซนเซอร์ X-Trans CMOS 5 HR 40MP ระบบกันสั่น IBIS 7 สต็อป สไตล์เรโทร', 53500.00,
                'https://images.unsplash.com/photo-1508921912186-1d1a45ebb3c1?auto=format&fit=crop&w=1200&q=80', true, false, NOW(), 'seed');
    END IF;

    -- Canon RF 50mm f/1.2L USM
    SELECT id INTO v_prod_canon FROM products WHERE store_id = v_store_id AND product_code = 'CANON-RF50-12';
    IF v_prod_canon IS NULL THEN
        v_prod_canon := gen_random_uuid();
        INSERT INTO products (id, store_id, brand_id, category_id, model_name, product_code, description, default_selling_price, image_url, is_active, is_deleted, created_at, created_by)
        VALUES (v_prod_canon, v_store_id, v_brand_canon, v_cat_lens, 'RF 50mm f/1.2L USM', 'CANON-RF50-12', 'เลนส์พอร์ตเทรตเกรดโปรรูรับแสงกว้างพิเศษ f/1.2 ชิ้นเลนส์ L-Series คมกริบ โบเก้กลมละมุน', 61900.00,
                'https://images.unsplash.com/photo-1617005082133-548c4dd27f35?auto=format&fit=crop&w=1200&q=80', true, false, NOW(), 'seed');
    END IF;

    -- --------------------------------------------------------------------------
    -- 6. ลบข้อมูลทดสอบเดิมของทั้ง 3 SKU (ถ้ามี) เพื่อให้รันซ้ำได้ไม่ error
    -- --------------------------------------------------------------------------
    DELETE FROM inventory_items WHERE store_id = v_store_id AND sku IN ('CAM-2610-0001', 'CAM-2610-0002', 'LEN-2610-0001');

    -- --------------------------------------------------------------------------
    -- 7. บันทึกสินค้าเข้าคลัง 3 ชิ้น (Inventory Items) — Status 3 = Available
    -- --------------------------------------------------------------------------
    -- ชิ้นที่ 1: Sony A7 IV
    INSERT INTO inventory_items (
        id, store_id, sku, product_id, serial_number, condition_grade_id, status,
        purchase_cost, actual_cost, selling_price, shutter_count,
        accessories, defects, notes, received_at, is_deleted, created_at, created_by
    ) VALUES (
        v_item_sony, v_store_id, 'CAM-2610-0001', v_prod_sony, '3849102', v_grade_aplus, 3,
        54000.00, 54000.00, 64900.00, 4520,
        'กล่องแท้ตรงรุ่น, แบตเตอรี่แท้ 2 ก้อน, แท่นชาร์จคู่, สายคล้องคอเดิมยังไม่แกะใช้, ฝาปิดบอดี้',
        'สภาพ 98% ไร้รอยขนแมว จอติดฟิล์มกระจกแล้ว ยางกริปแน่นสนิท เซนเซอร์สะอาดใส',
        'รับซื้อจากลูกค้าประจำหน้าร้าน ประกันศูนย์ไทยหมดแล้ว ใช้งานสมบูรณ์ 100%',
        NOW() - INTERVAL '3 days', false, NOW(), 'system'
    );

    -- ชิ้นที่ 2: Fujifilm X-T5
    INSERT INTO inventory_items (
        id, store_id, sku, product_id, serial_number, condition_grade_id, status,
        purchase_cost, actual_cost, selling_price, shutter_count,
        accessories, defects, notes, received_at, is_deleted, created_at, created_by
    ) VALUES (
        v_item_fuji, v_store_id, 'CAM-2610-0002', v_prod_fuji, '2837491', v_grade_a, 3,
        45000.00, 45000.00, 53500.00, 8900,
        'กล่องคู่มือครบ, แบตเตอรี่แท้ 1 ก้อน + แบตเทียบ 1 ก้อน, สายชาร์จ USB-C, ฮอทชูก้านล็อค',
        'มีรอยสะกิดเล็กๆ บริเวณขอบฐานกล้องด้านล่าง 1 จุด (ตามรูปถ่าย) จอและปุ่มทำงานปกติสมบูรณ์',
        'เซนเซอร์สะอาด ไดอัลหมุนคลิกแน่นทุกวง',
        NOW() - INTERVAL '5 days', false, NOW(), 'system'
    );

    -- ชิ้นที่ 3: Canon RF 50mm f/1.2L
    INSERT INTO inventory_items (
        id, store_id, sku, product_id, serial_number, condition_grade_id, status,
        purchase_cost, actual_cost, selling_price, shutter_count,
        accessories, defects, notes, received_at, is_deleted, created_at, created_by
    ) VALUES (
        v_item_canon, v_store_id, 'LEN-2610-0001', v_prod_canon, '8472910', v_grade_aplus, 3,
        51000.00, 51000.00, 61900.00, NULL,
        'ฮูดแท้ ES-83, ฝาปิดหน้า-หลัง Canon แท้, ถุงผ้ากำมะหยี่ LP1319, กล่องสีเทาเดิม',
        'ชิ้นแก้วใส ไร้ฝ้า ไร้รา โค้ทติ้งสวยสดใส กระบอกเลนส์ไร้รอย',
        'เลนส์อดีตประกันศูนย์ไทย โฟกัสรวดเร็วเงียบกริบ',
        NOW() - INTERVAL '1 days', false, NOW(), 'system'
    );

    -- --------------------------------------------------------------------------
    -- 8. ใส่รูปภาพแกลเลอรีให้แต่ละชิ้น (Inventory Item Images)
    -- --------------------------------------------------------------------------

    -- === แกลเลอรีชุดที่ 1: Sony A7 IV (5 รูป) ===
    INSERT INTO inventory_item_images (id, store_id, inventory_item_id, url, sort_order, created_at, created_by)
    VALUES
    (gen_random_uuid(), v_store_id, v_item_sony, 'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?auto=format&fit=crop&w=1200&q=80', 0, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_sony, 'https://images.unsplash.com/photo-1502982720700-bfff97f2ecac?auto=format&fit=crop&w=1200&q=80', 1, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_sony, 'https://images.unsplash.com/photo-1512790182412-b19e6d62bc39?auto=format&fit=crop&w=1200&q=80', 2, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_sony, 'https://images.unsplash.com/photo-1513694203232-719a280e022f?auto=format&fit=crop&w=1200&q=80', 3, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_sony, 'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?auto=format&fit=crop&w=1200&q=80', 4, NOW(), 'seed');

    -- === แกลเลอรีชุดที่ 2: Fujifilm X-T5 (4 รูป) ===
    INSERT INTO inventory_item_images (id, store_id, inventory_item_id, url, sort_order, created_at, created_by)
    VALUES
    (gen_random_uuid(), v_store_id, v_item_fuji, 'https://images.unsplash.com/photo-1508921912186-1d1a45ebb3c1?auto=format&fit=crop&w=1200&q=80', 0, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_fuji, 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=1200&q=80', 1, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_fuji, 'https://images.unsplash.com/photo-1495745966610-2a67f2297e5e?auto=format&fit=crop&w=1200&q=80', 2, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_fuji, 'https://images.unsplash.com/photo-1588702547919-26089e690ecc?auto=format&fit=crop&w=1200&q=80', 3, NOW(), 'seed');

    -- === แกลเลอรีชุดที่ 3: Canon RF 50mm f/1.2L (4 รูป) ===
    INSERT INTO inventory_item_images (id, store_id, inventory_item_id, url, sort_order, created_at, created_by)
    VALUES
    (gen_random_uuid(), v_store_id, v_item_canon, 'https://images.unsplash.com/photo-1617005082133-548c4dd27f35?auto=format&fit=crop&w=1200&q=80', 0, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_canon, 'https://images.unsplash.com/photo-1510127034890-ba27508e9f1c?auto=format&fit=crop&w=1200&q=80', 1, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_canon, 'https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=1200&q=80', 2, NOW(), 'seed'),
    (gen_random_uuid(), v_store_id, v_item_canon, 'https://images.unsplash.com/photo-1520390138845-fd2d229dd553?auto=format&fit=crop&w=1200&q=80', 3, NOW(), 'seed');

    RAISE NOTICE 'SUCCESS: Seeded 3 inventory items with full image galleries for Store %', v_store_id;
END $$;

-- --------------------------------------------------------------------------
-- ตรวจสอบผลลัพธ์ทันทีหลังรัน
-- --------------------------------------------------------------------------
SELECT 
    i.sku,
    p.model_name,
    cg.grade_code AS grade,
    i.serial_number,
    i.selling_price,
    i.status,
    COUNT(img.id) AS total_photos
FROM inventory_items i
JOIN products p ON i.product_id = p.id
JOIN condition_grades cg ON i.condition_grade_id = cg.id
LEFT JOIN inventory_item_images img ON img.inventory_item_id = i.id
WHERE i.sku IN ('CAM-2610-0001', 'CAM-2610-0002', 'LEN-2610-0001')
GROUP BY i.sku, p.model_name, cg.grade_code, i.serial_number, i.selling_price, i.status
ORDER BY i.sku;
